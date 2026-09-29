using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class CustomerAuthServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IPasswordHelper> _passwordHelperMock;
        private readonly Mock<IJwtHelper> _jwtHelperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CustomerAuthService _customerAuthService;

        public CustomerAuthServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _passwordHelperMock = new Mock<IPasswordHelper>();
            _jwtHelperMock = new Mock<IJwtHelper>();
            _mapperMock = new Mock<IMapper>();

            _customerAuthService = new CustomerAuthService(
                _customerRepositoryMock.Object,
                _passwordHelperMock.Object,
                _jwtHelperMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenCustomerDoesNotExist()
        {
            var request = new CustomerLoginRequestDto
            {
                Email = "notfound@example.com",
                Password = "Password123",
            };

            _customerRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((Customer?)null);

            var action = async () => await _customerAuthService.LoginAsync(request);

            await action
                .Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenCustomerIsInactive()
        {
            var request = new CustomerLoginRequestDto
            {
                Email = "inactive@example.com",
                Password = "Password123",
            };

            var customer = new Customer
            {
                Id = 1,
                Email = request.Email,
                PasswordHash = "hashedPassword",
                IsActive = false,
            };

            _customerRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(customer);

            var action = async () => await _customerAuthService.LoginAsync(request);

            await action
                .Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Your account is inactive.");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenPasswordIsInvalid()
        {
            var request = new CustomerLoginRequestDto
            {
                Email = "customer@example.com",
                Password = "WrongPassword",
            };

            var customer = new Customer
            {
                Id = 1,
                Email = request.Email,
                PasswordHash = "hashedPassword",
                IsActive = true,
            };

            _customerRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(customer);

            _passwordHelperMock
                .Setup(helper => helper.VerifyPassword(request.Password, customer.PasswordHash))
                .Returns(false);

            var action = async () => await _customerAuthService.LoginAsync(request);

            await action
                .Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnCustomerAndToken_WhenCredentialsAreValid()
        {
            var request = new CustomerLoginRequestDto
            {
                Email = "valid@example.com",
                Password = "CorrectPassword",
            };

            var customer = new Customer
            {
                Id = 5,
                FirstName = "Jane",
                LastName = "Doe",
                Email = request.Email,
                PasswordHash = "hashedPassword",
                IsActive = true,
            };

            var expectedResponse = new CustomerLoginResponseDto
            {
                Id = 5,
                FirstName = "Jane",
                LastName = "Doe",
                Email = request.Email,
                Role = "Customer",
            };

            _customerRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(customer);

            _passwordHelperMock
                .Setup(helper => helper.VerifyPassword(request.Password, customer.PasswordHash))
                .Returns(true);

            _jwtHelperMock
                .Setup(jwt => jwt.GenerateToken(customer.Id, customer.Email, "Customer"))
                .Returns("jwt-customer-token-123");

            _mapperMock
                .Setup(mapper => mapper.Map<CustomerLoginResponseDto>(customer))
                .Returns(expectedResponse);

            var result = await _customerAuthService.LoginAsync(request);

            result.Customer.Should().NotBeNull();
            result.Customer.Id.Should().Be(5);
            result.Customer.FirstName.Should().Be("Jane");
            result.Customer.LastName.Should().Be("Doe");
            result.Customer.Email.Should().Be("valid@example.com");
            result.Customer.Role.Should().Be("Customer");
            result.Token.Should().Be("jwt-customer-token-123");
        }
    }
}
