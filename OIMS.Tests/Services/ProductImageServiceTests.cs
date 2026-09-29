using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class ProductImageServiceTests
    {
        private readonly Mock<IProductImageRepository> _productImageRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly ProductImageService _productImageService;

        public ProductImageServiceTests()
        {
            _productImageRepositoryMock = new Mock<IProductImageRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _productImageService = new ProductImageService(
                _productImageRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        private IFormFile CreateFormFile(string fileName, string contentType, long sizeInBytes)
        {
            var content = new byte[sizeInBytes];
            var stream = new MemoryStream(content);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(sizeInBytes);
            fileMock
                .Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            return fileMock.Object;
        }

        private void SetupHttpContext(string scheme = "https", string host = "localhost:5001")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = scheme;
            httpContext.Request.Host = new HostString(host);

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
        {
            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(999))
                .ReturnsAsync((Product?)null);

            var files = new List<IFormFile> { CreateFormFile("test.png", "image/png", 1024) };

            var action = async () =>
                await _productImageService.UploadImagesAsync(999, files, userId: 1);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowBadRequestException_WhenFilesListIsNull()
        {
            var product = new Product { Id = 1, Name = "Item" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(1))
                .ReturnsAsync(product);

            var action = async () =>
                await _productImageService.UploadImagesAsync(1, null!, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("At least one image is required.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowBadRequestException_WhenFilesListIsEmpty()
        {
            var product = new Product { Id = 1, Name = "Item" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(1))
                .ReturnsAsync(product);

            var emptyFiles = new List<IFormFile>();

            var action = async () =>
                await _productImageService.UploadImagesAsync(1, emptyFiles, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("At least one image is required.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowBadRequestException_WhenExceedingMaxImageCount()
        {
            var product = new Product { Id = 1, Name = "Item" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(1))
                .ReturnsAsync(product);

            _productImageRepositoryMock.Setup(repo => repo.GetImageCountAsync(1)).ReturnsAsync(2);

            var files = new List<IFormFile>
            {
                CreateFormFile("img1.png", "image/png", 1024),
                CreateFormFile("img2.png", "image/png", 1024),
            };

            var action = async () =>
                await _productImageService.UploadImagesAsync(1, files, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("A product can have a maximum of 3 images.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowBadRequestException_WhenFileIsEmpty()
        {
            var product = new Product { Id = 1, Name = "Item" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(1))
                .ReturnsAsync(product);

            _productImageRepositoryMock.Setup(repo => repo.GetImageCountAsync(1)).ReturnsAsync(0);

            var files = new List<IFormFile> { CreateFormFile("empty.png", "image/png", 0) };

            var action = async () =>
                await _productImageService.UploadImagesAsync(1, files, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Image file cannot be empty.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowBadRequestException_WhenFileSizeExceeds5Mb()
        {
            var product = new Product { Id = 1, Name = "Item" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(1))
                .ReturnsAsync(product);

            _productImageRepositoryMock.Setup(repo => repo.GetImageCountAsync(1)).ReturnsAsync(0);

            long oversizedBytes = (5 * 1024 * 1024) + 1;
            var files = new List<IFormFile>
            {
                CreateFormFile("huge.png", "image/png", oversizedBytes),
            };

            var action = async () =>
                await _productImageService.UploadImagesAsync(1, files, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Each image must be less than or equal to 5 MB.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowBadRequestException_WhenFileExtensionIsNotAllowed()
        {
            var product = new Product { Id = 1, Name = "Item" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(1))
                .ReturnsAsync(product);

            _productImageRepositoryMock.Setup(repo => repo.GetImageCountAsync(1)).ReturnsAsync(0);

            var files = new List<IFormFile> { CreateFormFile("document.pdf", "image/png", 2048) };

            var action = async () =>
                await _productImageService.UploadImagesAsync(1, files, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldThrowBadRequestException_WhenContentTypeIsNotAllowed()
        {
            var product = new Product { Id = 1, Name = "Item" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(1))
                .ReturnsAsync(product);

            _productImageRepositoryMock.Setup(repo => repo.GetImageCountAsync(1)).ReturnsAsync(0);

            var files = new List<IFormFile>
            {
                CreateFormFile("test.png", "application/octet-stream", 2048),
            };

            var action = async () =>
                await _productImageService.UploadImagesAsync(1, files, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Invalid image content type.");
        }

        [Fact]
        public async Task UploadImagesAsync_ShouldSaveImagesAndReturnFullUrls_WhenFilesAreValid()
        {
            SetupHttpContext("https", "myapi.com");

            var product = new Product { Id = 5, Name = "Mouse" };

            _productImageRepositoryMock
                .Setup(repo => repo.GetProductAsync(5))
                .ReturnsAsync(product);

            _productImageRepositoryMock.Setup(repo => repo.GetImageCountAsync(5)).ReturnsAsync(0);

            _productImageRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<ProductImage>()))
                .Returns(Task.CompletedTask);

            _productImageRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var files = new List<IFormFile> { CreateFormFile("mouse.png", "image/png", 4096) };

            var result = await _productImageService.UploadImagesAsync(5, files, userId: 7);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].ProductId.Should().Be(5);
            result[0].OriginalFileName.Should().Be("mouse.png");
            result[0].ContentType.Should().Be("image/png");
            result[0].FileUrl.Should().StartWith("https://myapi.com/uploads/products/5/");

            _productImageRepositoryMock.Verify(
                repo =>
                    repo.AddAsync(
                        It.Is<ProductImage>(img =>
                            img.ProductId == 5
                            && img.OriginalFileName == "mouse.png"
                            && img.CreatedBy == 7
                            && !img.IsDeleted
                        )
                    ),
                Times.Once
            );

            _productImageRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            string cleanUpFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "products",
                "5"
            );
            if (Directory.Exists(cleanUpFolder))
            {
                Directory.Delete(cleanUpFolder, recursive: true);
            }
        }
    }
}
