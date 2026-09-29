using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OIMS.API.Controllers;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Services;
using Xunit;

namespace OIMS.Tests.Controllers
{
    public class CustomersControllerTests
    {
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly CustomersController _controller;

        public CustomersControllerTests()
        {
            _customerServiceMock = new Mock<ICustomerService>();
            _controller = new CustomersController(_customerServiceMock.Object);
        }

        private void SetupUserClaims(int userId, string role = "Administrator")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("role", role),
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };
        }

        [Fact]
        public async Task CreateCustomer_ShouldReturn201Created_WhenRequestIsValid()
        {
            int userId = 10;
            SetupUserClaims(userId);

            var request = new CreateCustomerRequestDto
            {
                Email = "customer@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
            };

            var serviceResponse = new CreatedCustomerResponseDto
            {
                Id = 1,
                Email = "customer@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _customerServiceMock
                .Setup(s => s.CreateCustomerAsync(request, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.CreateCustomer(request);

            var objectResult = actionResult as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(StatusCodes.Status201Created);

            var apiResponse = objectResult.Value as ApiResponse<CreatedCustomerResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Customer created successfully.");

            _customerServiceMock.Verify(s => s.CreateCustomerAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task GetCustomers_ShouldReturnOkWithPagedResponse_WhenCalled()
        {
            int userId = 25;
            string role = "Manager";
            SetupUserClaims(userId, role);

            var request = new GetCustomersRequestDto
            {
                Page = 1,
                SortBy = "name",
                SortOrder = "asc",
            };

            var pagedResponse = new PagedResponseDto<CustomerListResponseDto>
            {
                Data = new List<CustomerListResponseDto>
                {
                    new CustomerListResponseDto
                    {
                        Id = 1,
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john@example.com",
                        IsActive = true,
                    },
                },
                Page = 1,
                PageSize = 10,
                TotalRecords = 1,
                TotalPages = 1,
            };

            _customerServiceMock
                .Setup(s => s.GetCustomersAsync(request, role, userId))
                .ReturnsAsync(pagedResponse);

            var actionResult = await _controller.GetCustomers(request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse =
                okResult.Value as ApiResponse<PagedResponseDto<CustomerListResponseDto>>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(pagedResponse);
            apiResponse.Message.Should().Be("Customers retrieved successfully.");

            _customerServiceMock.Verify(
                s => s.GetCustomersAsync(request, role, userId),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateCustomer_ShouldReturnOk_WhenUpdateSucceeds()
        {
            int customerId = 1;
            int userId = 10;
            SetupUserClaims(userId);

            var request = new UpdateCustomerRequestDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Phone = "9876543210",
            };

            var serviceResponse = new CreatedCustomerResponseDto
            {
                Id = customerId,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "customer@example.com",
                Phone = "9876543210",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _customerServiceMock
                .Setup(s => s.UpdateCustomerAsync(customerId, request, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.UpdateCustomer(customerId, request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<CreatedCustomerResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Customer updated successfully.");

            _customerServiceMock.Verify(
                s => s.UpdateCustomerAsync(customerId, request, userId),
                Times.Once
            );
        }

        [Fact]
        public async Task DeactivateCustomer_ShouldReturnOk_WhenDeactivatedSuccessfully()
        {
            int customerId = 2;
            int userId = 10;
            SetupUserClaims(userId);

            _customerServiceMock
                .Setup(s => s.DeactiveCustomersAsync(customerId, userId))
                .Returns(Task.CompletedTask);

            var actionResult = await _controller.DeactivateCustomer(customerId);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<bool>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeTrue();
            apiResponse.Message.Should().Be("Customer deactivated successfully.");

            _customerServiceMock.Verify(
                s => s.DeactiveCustomersAsync(customerId, userId),
                Times.Once
            );
        }
    }
}
