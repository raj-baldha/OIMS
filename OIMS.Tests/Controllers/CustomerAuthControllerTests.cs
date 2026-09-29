using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using OIMS.API.Controllers;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Services;
using Xunit;

namespace OIMS.Tests.Controllers
{
    public class CustomerAuthControllerTests
    {
        private readonly Mock<ICustomerAuthService> _customerAuthServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly CustomerAuthController _controller;
        private readonly DefaultHttpContext _httpContext;

        public CustomerAuthControllerTests()
        {
            _customerAuthServiceMock = new Mock<ICustomerAuthService>();
            _configurationMock = new Mock<IConfiguration>();

            _controller = new CustomerAuthController(
                _customerAuthServiceMock.Object,
                _configurationMock.Object
            );

            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = _httpContext };
        }

        [Fact]
        public async Task Login_ShouldReturnOkWithCustomerAndSetCookie_WhenCredentialsAreValid()
        {
            var request = new CustomerLoginRequestDto
            {
                Email = "customer@example.com",
                Password = "Password123!",
            };

            var customerResponse = new CustomerLoginResponseDto
            {
                Id = 10,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "customer@example.com",
                Role = "Customer",
            };

            string token = "jwt-customer-token";

            _customerAuthServiceMock
                .Setup(s => s.LoginAsync(request))
                .ReturnsAsync((customerResponse, token));

            _configurationMock.Setup(c => c["Jwt:DurationInMinutes"]).Returns("60");

            var actionResult = await _controller.Login(request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<CustomerLoginResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(customerResponse);
            apiResponse.Message.Should().Be("Customer login successful.");

            _httpContext.Response.Headers.ContainsKey("Set-Cookie").Should().BeTrue();
            _customerAuthServiceMock.Verify(s => s.LoginAsync(request), Times.Once);
        }

        [Fact]
        public void Logout_ShouldReturnOkAndAppendExpiredCookie()
        {
            var actionResult = _controller.Logout();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Message.Should().Be("Customer logout successful.");

            _httpContext.Response.Headers.ContainsKey("Set-Cookie").Should().BeTrue();
        }
    }
}
