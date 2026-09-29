using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IPasswordHelper> _passwordHelperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _passwordHelperMock = new Mock<IPasswordHelper>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _mapperMock = new Mock<IMapper>();

            _customerService = new CustomerService(
                _customerRepositoryMock.Object,
                _passwordHelperMock.Object,
                _emailServiceMock.Object,
                _configurationMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
        {
            var request = new CreateCustomerRequestDto
            {
                Email = "duplicate@example.com",
                Password = "Password123",
            };

            _customerRepositoryMock
                .Setup(x => x.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(true);

            var action = async () => await _customerService.CreateCustomerAsync(request, userId: 1);

            await action
                .Should()
                .ThrowAsync<ConflictException>()
                .WithMessage("Email already exists.");

            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Never);
            _customerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            _emailServiceMock.Verify(
                x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldCreateCustomerAndSendEmail_WhenEmailIsUnique()
        {
            var request = new CreateCustomerRequestDto
            {
                Email = "unique@example.com",
                Password = "PlainPassword123",
                FirstName = "Alice",
                LastName = "Wonder",
                Phone = "1234567890",
                Address = "Baker St",
                City = "London",
                State = "UK",
                Country = "UK",
            };

            var templateDir = Path.Combine(AppContext.BaseDirectory, "EmailTemplate");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, "WelcomeEmail.html");
            await File.WriteAllTextAsync(
                templatePath,
                "Hello {{Username}}, email: {{Email}}, pass: {{Password}}, role: {{Role}}"
            );

            _customerRepositoryMock
                .Setup(x => x.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(false);

            _passwordHelperMock
                .Setup(x => x.HashPassword(request.Password))
                .Returns("hashedPassword123");

            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            _customerRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(x => x.SendEmailAsync(request.Email, "Welcome to OIMS", It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _customerService.CreateCustomerAsync(request, userId: 5);

            result.Should().NotBeNull();
            result.Email.Should().Be("unique@example.com");
            result.FirstName.Should().Be("Alice");
            result.LastName.Should().Be("Wonder");
            result.IsActive.Should().BeTrue();

            _customerRepositoryMock.Verify(
                x =>
                    x.AddAsync(
                        It.Is<Customer>(c =>
                            c.Email == request.Email
                            && c.PasswordHash == "hashedPassword123"
                            && c.CreatedBy == 5
                        )
                    ),
                Times.Once
            );

            _customerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            _emailServiceMock.Verify(
                x =>
                    x.SendEmailAsync(
                        request.Email,
                        "Welcome to OIMS",
                        It.Is<string>(body =>
                            body.Contains("Alice Wonder") && body.Contains("PlainPassword123")
                        )
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldPassCustomerId_WhenRoleIsCustomer()
        {
            var request = new GetCustomersRequestDto
            {
                Page = 1,
                SortBy = "name",
                SortOrder = "asc",
            };

            _configurationMock.Setup(x => x["Pagination:PageSize"]).Returns("10");

            var expectedResponse = new PagedResponseDto<CustomerListResponseDto>
            {
                Data = new List<CustomerListResponseDto>(),
                Page = 1,
                PageSize = 10,
                TotalRecords = 0,
                TotalPages = 0,
            };

            _customerRepositoryMock
                .Setup(x => x.GetCustomersAsync(request, 10, 25))
                .ReturnsAsync(expectedResponse);

            var result = await _customerService.GetCustomersAsync(
                request,
                role: "Customer",
                userId: 25
            );

            result.Should().NotBeNull();
            _customerRepositoryMock.Verify(x => x.GetCustomersAsync(request, 10, 25), Times.Once);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldPassNullCustomerIdAndSetDefaults_WhenRoleIsNotCustomer()
        {
            var request = new GetCustomersRequestDto
            {
                Page = 0,
                SortBy = null,
                SortOrder = null,
            };

            _configurationMock.Setup(x => x["Pagination:PageSize"]).Returns("15");

            var expectedResponse = new PagedResponseDto<CustomerListResponseDto>
            {
                Data = new List<CustomerListResponseDto>(),
                Page = 1,
                PageSize = 15,
                TotalRecords = 0,
                TotalPages = 0,
            };

            _customerRepositoryMock
                .Setup(x => x.GetCustomersAsync(request, 15, null))
                .ReturnsAsync(expectedResponse);

            var result = await _customerService.GetCustomersAsync(
                request,
                role: "Admin",
                userId: 99
            );

            result.Should().NotBeNull();
            request.Page.Should().Be(1);
            request.SortBy.Should().Be("name");
            request.SortOrder.Should().Be("asc");

            _customerRepositoryMock.Verify(x => x.GetCustomersAsync(request, 15, null), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
        {
            var request = new UpdateCustomerRequestDto { FirstName = "Updated", LastName = "Name" };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Customer?)null);

            var action = async () =>
                await _customerService.UpdateCustomerAsync(999, request, userId: 1);

            await action
                .Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Customer not found.");

            _customerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldUpdateFieldsAndSave_WhenCustomerExists()
        {
            var existingCustomer = new Customer
            {
                Id = 1,
                Email = "exist@example.com",
                FirstName = "Old",
                LastName = "Name",
                Phone = "111",
                Address = "Old Address",
                City = "Old City",
                State = "Old State",
                Country = "Old Country",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
            };

            var request = new UpdateCustomerRequestDto
            {
                FirstName = "NewFirst",
                LastName = "NewLast",
                Phone = "999",
                Address = "New Address",
                City = "New City",
                State = "New State",
                Country = "New Country",
            };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingCustomer);

            _customerRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _customerService.UpdateCustomerAsync(1, request, userId: 10);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.FirstName.Should().Be("NewFirst");
            result.LastName.Should().Be("NewLast");
            result.City.Should().Be("New City");

            existingCustomer.FirstName.Should().Be("NewFirst");
            existingCustomer.LastName.Should().Be("NewLast");
            existingCustomer.Phone.Should().Be("999");
            existingCustomer.Address.Should().Be("New Address");
            existingCustomer.City.Should().Be("New City");
            existingCustomer.State.Should().Be("New State");
            existingCustomer.Country.Should().Be("New Country");
            existingCustomer.UpdatedBy.Should().Be(10);
            existingCustomer.UpdatedAt.Should().NotBeNull();

            _customerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactiveCustomersAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
        {
            _customerRepositoryMock.Setup(x => x.GetByIdAsync(404)).ReturnsAsync((Customer?)null);

            var action = async () => await _customerService.DeactiveCustomersAsync(404, userId: 1);

            await action
                .Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Customer not found.");

            _customerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeactiveCustomersAsync_ShouldSetIsActiveToFalseAndSave_WhenCustomerExists()
        {
            var existingCustomer = new Customer
            {
                Id = 1,
                Email = "active@example.com",
                IsActive = true,
            };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingCustomer);

            _customerRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _customerService.DeactiveCustomersAsync(1, userId: 20);

            existingCustomer.IsActive.Should().BeFalse();
            existingCustomer.UpdatedBy.Should().Be(20);
            existingCustomer.UpdatedAt.Should().NotBeNull();

            _customerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldSucceedAndCoverAllProperties_WhenTemplateExists()
        {
            var request = new CreateCustomerRequestDto
            {
                Email = "fullcustomer@test.com",
                Password = "Password123!",
                FirstName = "Bruce",
                LastName = "Wayne",
                Phone = "123456789",
                Address = "1007 Mountain Drive",
                City = "Gotham",
                State = "NJ",
                Country = "USA",
            };

            var templateDir = Path.Combine(AppContext.BaseDirectory, "EmailTemplate");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, "WelcomeEmail.html");
            await File.WriteAllTextAsync(
                templatePath,
                "Hello {{Username}}, {{Email}}, {{Role}}, {{Password}}"
            );

            _customerRepositoryMock
                .Setup(x => x.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(false);

            _passwordHelperMock.Setup(x => x.HashPassword(request.Password)).Returns("secureHash");

            _customerRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            _customerRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(x => x.SendEmailAsync(request.Email, "Welcome to OIMS", It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _customerService.CreateCustomerAsync(request, userId: 42);

            result.Should().NotBeNull();
            result.FirstName.Should().Be("Bruce");
            result.LastName.Should().Be("Wayne");
            result.Email.Should().Be("fullcustomer@test.com");
            result.Phone.Should().Be("123456789");
            result.Address.Should().Be("1007 Mountain Drive");
            result.City.Should().Be("Gotham");
            result.State.Should().Be("NJ");
            result.Country.Should().Be("USA");
            result.IsActive.Should().BeTrue();

            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Once);
            _customerRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _emailServiceMock.Verify(
                x => x.SendEmailAsync(request.Email, "Welcome to OIMS", It.IsAny<string>()),
                Times.Once
            );
        }
    }
}
