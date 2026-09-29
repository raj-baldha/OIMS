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
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthController _controller;
        private readonly DefaultHttpContext _httpContext;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _configurationMock = new Mock<IConfiguration>();

            _controller = new AuthController(_authServiceMock.Object, _configurationMock.Object);

            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = _httpContext };
        }

        [Fact]
        public async Task Login_ShouldReturnOkWithUserAndSetCookie_WhenCredentialsAreValid()
        {
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!",
            };

            var userResponse = new LoginResponseDto
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Role = "Employee",
            };

            string token = "jwt-test-token";

            _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync((userResponse, token));

            _configurationMock.Setup(c => c["Jwt:DurationInMinutes"]).Returns("60");

            var actionResult = await _controller.Login(request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<LoginResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(userResponse);
            apiResponse.Message.Should().Be("Login successful.");

            _httpContext.Response.Headers.ContainsKey("Set-Cookie").Should().BeTrue();
            _authServiceMock.Verify(s => s.LoginAsync(request), Times.Once);
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
            apiResponse!.Message.Should().Be("Logout successful.");

            _httpContext.Response.Headers.ContainsKey("Set-Cookie").Should().BeTrue();
        }
    }
}
