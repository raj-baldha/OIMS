using System;
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
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _controller = new CategoriesController(_categoryServiceMock.Object);
        }

        private void SetupUserClaims(int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("id", userId.ToString()),
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };
        }

        [Fact]
        public async Task CreateCategory_ShouldReturn201Created_WhenRequestIsValid()
        {
            int userId = 5;
            SetupUserClaims(userId);

            var request = new CreateCategoryRequestDto { Name = "Laptops" };

            var serviceResponse = new CreatedCategoryResponseDto
            {
                Id = 1,
                Name = "Laptops",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
            };

            _categoryServiceMock
                .Setup(s => s.CreateCategoryAsync(request, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.CreateCategory(request);

            var objectResult = actionResult as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(StatusCodes.Status201Created);

            var apiResponse = objectResult.Value as ApiResponse<CreatedCategoryResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Category created successfully.");

            _categoryServiceMock.Verify(s => s.CreateCategoryAsync(request, userId), Times.Once);
        }
    }
}
