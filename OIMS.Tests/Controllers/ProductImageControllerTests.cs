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
using OIMS.Application.DTOs.Products;
using OIMS.Application.Interfaces.Services;
using Xunit;

namespace OIMS.Tests.Controllers
{
    public class ProductImageControllerTests
    {
        private readonly Mock<IProductImageService> _productImageServiceMock;
        private readonly ProductImageController _controller;

        public ProductImageControllerTests()
        {
            _productImageServiceMock = new Mock<IProductImageService>();
            _controller = new ProductImageController(_productImageServiceMock.Object);
        }

        private void SetupUserClaims(int userId, string role = "Employee")
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
        public async Task UploadImages_ShouldReturnOkWithUploadedImages_WhenUploadSucceeds()
        {
            int productId = 10;
            int userId = 4;
            SetupUserClaims(userId);

            var files = new List<IFormFile>();

            var serviceResponse = new List<ProductImageResponseDto>
            {
                new ProductImageResponseDto
                {
                    Id = 1,
                    ProductId = productId,
                    FileUrl = "https://localhost:5001/uploads/products/10/img1.png",
                    OriginalFileName = "img1.png",
                    ContentType = "image/png",
                    FileSize = 1024,
                    CreatedAt = DateTime.UtcNow,
                },
            };

            _productImageServiceMock
                .Setup(s => s.UploadImagesAsync(productId, files, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.UploadImages(productId, files);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<List<ProductImageResponseDto>>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Product images uploaded successfully.");

            _productImageServiceMock.Verify(
                s => s.UploadImagesAsync(productId, files, userId),
                Times.Once
            );
        }
    }
}
