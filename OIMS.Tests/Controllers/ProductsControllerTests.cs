using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OIMS.API.Controllers;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IValidator<CreateProductRequestDto>> _validatorMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _validatorMock = new Mock<IValidator<CreateProductRequestDto>>();

            _controller = new ProductsController(_productServiceMock.Object, _validatorMock.Object);
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
        public async Task CreateProduct_ShouldThrowBadRequestException_WhenValidationFails()
        {
            var request = new CreateProductRequestDto();

            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required."),
                new ValidationFailure("Sku", "SKU is required."),
            };

            var validationResult = new ValidationResult(failures);

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var action = async () => await _controller.CreateProduct(request);

            var exception = await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Validation failed.");

            exception.Which.Errors.Should().Contain("Name is required.");
            exception.Which.Errors.Should().Contain("SKU is required.");

            _productServiceMock.Verify(
                s => s.CreateProductAsync(It.IsAny<CreateProductRequestDto>(), It.IsAny<int>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CreateProduct_ShouldReturn201Created_WhenRequestIsValid()
        {
            int userId = 10;
            SetupUserClaims(userId);

            var request = new CreateProductRequestDto
            {
                Name = "Gaming Mouse",
                Sku = "SKU-GM-01",
                CategoryId = 1,
                SellingPrice = 50,
                CostPrice = 30,
                QuantityOnHand = 20,
                MinStockLevel = 5,
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var serviceResponse = new CreatedProductResponseDto
            {
                Id = 1,
                Name = request.Name,
                Sku = request.Sku,
                CategoryId = request.CategoryId,
                SellingPrice = request.SellingPrice,
                CostPrice = request.CostPrice,
                QuantityOnHand = request.QuantityOnHand,
                MinStockLevel = request.MinStockLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
            };

            _productServiceMock
                .Setup(s => s.CreateProductAsync(request, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.CreateProduct(request);

            var objectResult = actionResult as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(StatusCodes.Status201Created);

            var apiResponse = objectResult.Value as ApiResponse<CreatedProductResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Product created successfully.");

            _productServiceMock.Verify(s => s.CreateProductAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnOk_WhenUpdateSucceeds()
        {
            int productId = 1;
            int userId = 10;
            SetupUserClaims(userId);

            var request = new UpdateProductRequestDto
            {
                Name = "Updated Keyboard",
                Sku = "SKU-KEY-02",
                CategoryId = 2,
                SellingPrice = 75,
                CostPrice = 45,
                MinStockLevel = 3,
                IsActive = true,
            };

            var serviceResponse = new CreatedProductResponseDto
            {
                Id = productId,
                Name = request.Name,
                Sku = request.Sku,
                CategoryId = request.CategoryId,
                SellingPrice = request.SellingPrice,
                CostPrice = request.CostPrice,
                MinStockLevel = request.MinStockLevel,
                IsActive = request.IsActive,
            };

            _productServiceMock
                .Setup(s => s.UpdateProductAsync(productId, request, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.UpdateProduct(productId, request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<CreatedProductResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Product updated successfully.");

            _productServiceMock.Verify(
                s => s.UpdateProductAsync(productId, request, userId),
                Times.Once
            );
        }

        [Fact]
        public async Task AdjustInventory_ShouldReturnOk_WhenAdjustmentSucceeds()
        {
            int productId = 1;
            int userId = 10;
            SetupUserClaims(userId);

            var request = new AdjustInventoryRequestDto
            {
                QuantityChange = 10,
                Notes = "Stock delivery",
            };

            var serviceResponse = new CreatedProductResponseDto
            {
                Id = productId,
                Name = "Monitor",
                QuantityOnHand = 25,
            };

            _productServiceMock
                .Setup(s => s.AdjustInventoryAsync(productId, request, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.AdjustInventory(productId, request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<CreatedProductResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Inventory adjusted successfully.");

            _productServiceMock.Verify(
                s => s.AdjustInventoryAsync(productId, request, userId),
                Times.Once
            );
        }

        [Fact]
        public async Task DeactivateProduct_ShouldReturnOk_WhenDeactivatedSuccessfully()
        {
            int productId = 1;
            int userId = 10;
            SetupUserClaims(userId);

            var serviceResponse = new DeactivatedProductResponseDto
            {
                Id = productId,
                Name = "Old Headset",
                Sku = "SKU-OLD-01",
                IsActive = false,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId,
            };

            _productServiceMock
                .Setup(s => s.DeactivateProductAsync(productId, userId))
                .ReturnsAsync(serviceResponse);

            var actionResult = await _controller.DeactivateProduct(productId);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<DeactivatedProductResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(serviceResponse);
            apiResponse.Message.Should().Be("Product deactivated successfully.");

            _productServiceMock.Verify(
                s => s.DeactivateProductAsync(productId, userId),
                Times.Once
            );
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkWithPagedProducts_WhenCalled()
        {
            var request = new GetProductsRequestDto { Page = 1 };

            var pagedResponse = new PagedResponseDto<ProductListResponseDto>
            {
                Data = new List<ProductListResponseDto>
                {
                    new ProductListResponseDto
                    {
                        Id = 1,
                        Name = "Desk",
                        Sku = "SKU-DESK-01",
                        CategoryId = 1,
                        CategoryName = "Furniture",
                        SellingPrice = 120,
                        QuantityOnHand = 5,
                        MinStockLevel = 2,
                        IsActive = true,
                    },
                },
                Page = 1,
                PageSize = 10,
                TotalRecords = 1,
                TotalPages = 1,
            };

            _productServiceMock.Setup(s => s.GetProductsAsync(request)).ReturnsAsync(pagedResponse);

            var actionResult = await _controller.GetProducts(request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse =
                okResult.Value as ApiResponse<PagedResponseDto<ProductListResponseDto>>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(pagedResponse);
            apiResponse.Message.Should().Be("Products retrieved successfully.");

            _productServiceMock.Verify(s => s.GetProductsAsync(request), Times.Once);
        }
    }
}
