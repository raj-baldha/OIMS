using System;
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
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task CreateUser_ShouldReturn201Created_WhenUserIsCreatedSuccessfully()
        {
            var request = new CreateUserRequestDto
            {
                Username = "newadmin",
                Email = "admin@example.com",
                Password = "Password123!",
                Role = "Administrator",
            };

            var serviceResponse = new CreatedUserResponseDto
            {
                Id = 1,
                Username = "newadmin",
                Email = "admin@example.com",
                Role = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _userServiceMock.Setup(s => s.CreateUserAsync(request)).ReturnsAsync(serviceResponse);

            var actionResult = await _controller.CreateUser(request);

            var objectResult = actionResult as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(StatusCodes.Status201Created);

            var apiResponse = objectResult.Value as ApiResponse<CreatedUserResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("User created successfully.");

            _userServiceMock.Verify(s => s.CreateUserAsync(request), Times.Once);
        }
    }
}
