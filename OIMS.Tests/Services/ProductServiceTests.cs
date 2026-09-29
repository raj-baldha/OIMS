using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _configurationMock = new Mock<IConfiguration>();

            _productService = new ProductService(
                _productRepositoryMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task CreateProductAsync_ShouldThrowConflictException_WhenSkuAlreadyExists()
        {
            var request = new CreateProductRequestDto
            {
                Name = "Mouse",
                Sku = "  SKU-001  ",
                CategoryId = 1,
            };

            _productRepositoryMock
                .Setup(repo => repo.IsSkuExistsAsync("SKU-001"))
                .ReturnsAsync(true);

            var action = async () => await _productService.CreateProductAsync(request, userId: 1);

            await action
                .Should()
                .ThrowAsync<ConflictException>()
                .WithMessage("SKU already exists.");

            _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
        {
            var request = new CreateProductRequestDto
            {
                Name = "Mouse",
                Sku = "SKU-001",
                CategoryId = 99,
            };

            _productRepositoryMock
                .Setup(repo => repo.IsSkuExistsAsync(request.Sku))
                .ReturnsAsync(false);

            _productRepositoryMock
                .Setup(repo => repo.IsCategoryExistsAsync(request.CategoryId))
                .ReturnsAsync(false);

            var action = async () => await _productService.CreateProductAsync(request, userId: 1);

            await action
                .Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Category not found.");

            _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldAddProductAndReturnResponse_WhenValid()
        {
            var request = new CreateProductRequestDto
            {
                Name = " Keyboard ",
                Sku = "  SKU-002  ",
                CategoryId = 1,
                SellingPrice = 50,
                CostPrice = 30,
                QuantityOnHand = 10,
                MinStockLevel = 2,
            };

            _productRepositoryMock
                .Setup(repo => repo.IsSkuExistsAsync("SKU-002"))
                .ReturnsAsync(false);

            _productRepositoryMock.Setup(repo => repo.IsCategoryExistsAsync(1)).ReturnsAsync(true);

            _productRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _productRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _productService.CreateProductAsync(request, userId: 5);

            result.Should().NotBeNull();
            result.Name.Should().Be("Keyboard");
            result.Sku.Should().Be("SKU-002");
            result.CategoryId.Should().Be(1);
            result.SellingPrice.Should().Be(50);
            result.CostPrice.Should().Be(30);
            result.QuantityOnHand.Should().Be(10);
            result.MinStockLevel.Should().Be(2);
            result.IsActive.Should().BeTrue();
            result.CreatedBy.Should().Be(5);

            _productRepositoryMock.Verify(
                repo =>
                    repo.AddAsync(
                        It.Is<Product>(p =>
                            p.Name == "Keyboard"
                            && p.Sku == "SKU-002"
                            && p.CreatedBy == 5
                            && p.Version == 1
                            && !p.IsDeleted
                        )
                    ),
                Times.Once
            );

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
        {
            var request = new UpdateProductRequestDto
            {
                Name = "Updated",
                Sku = "SKU-999",
                CategoryId = 1,
            };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            var action = async () =>
                await _productService.UpdateProductAsync(999, request, userId: 1);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found.");

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowConflictException_WhenSkuExistsForOtherProduct()
        {
            var product = new Product { Id = 1, Sku = "SKU-OLD" };
            var request = new UpdateProductRequestDto
            {
                Name = "Updated",
                Sku = "  SKU-OTHER  ",
                CategoryId = 1,
            };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repo => repo.IsSkuExistsForOtherProductAsync("SKU-OTHER", 1))
                .ReturnsAsync(true);

            var action = async () =>
                await _productService.UpdateProductAsync(1, request, userId: 1);

            await action
                .Should()
                .ThrowAsync<ConflictException>()
                .WithMessage("SKU already exists.");

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
        {
            var product = new Product { Id = 1, Sku = "SKU-001" };
            var request = new UpdateProductRequestDto
            {
                Name = "Updated",
                Sku = "SKU-001",
                CategoryId = 99,
            };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repo => repo.IsSkuExistsForOtherProductAsync("SKU-001", 1))
                .ReturnsAsync(false);

            _productRepositoryMock
                .Setup(repo => repo.IsCategoryExistsAsync(99))
                .ReturnsAsync(false);

            var action = async () =>
                await _productService.UpdateProductAsync(1, request, userId: 1);

            await action
                .Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Category not found.");

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateFieldsAndIncrementVersion_WhenValid()
        {
            var product = new Product
            {
                Id = 1,
                Name = "Old",
                Sku = "SKU-OLD",
                CategoryId = 1,
                SellingPrice = 10,
                CostPrice = 5,
                MinStockLevel = 1,
                QuantityOnHand = 20,
                IsActive = true,
                Version = 1,
            };

            var request = new UpdateProductRequestDto
            {
                Name = " New Name ",
                Sku = " SKU-NEW ",
                CategoryId = 2,
                SellingPrice = 25,
                CostPrice = 15,
                MinStockLevel = 5,
                IsActive = false,
            };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repo => repo.IsSkuExistsForOtherProductAsync("SKU-NEW", 1))
                .ReturnsAsync(false);

            _productRepositoryMock.Setup(repo => repo.IsCategoryExistsAsync(2)).ReturnsAsync(true);

            _productRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _productService.UpdateProductAsync(1, request, userId: 10);

            result.Should().NotBeNull();
            result.Name.Should().Be("New Name");
            result.Sku.Should().Be("SKU-NEW");
            result.CategoryId.Should().Be(2);
            result.SellingPrice.Should().Be(25);
            result.CostPrice.Should().Be(15);
            result.MinStockLevel.Should().Be(5);
            result.IsActive.Should().BeFalse();

            product.Name.Should().Be("New Name");
            product.Sku.Should().Be("SKU-NEW");
            product.Version.Should().Be(2);
            product.UpdatedBy.Should().Be(10);
            product.UpdatedAt.Should().NotBeNull();

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AdjustInventoryAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
        {
            var request = new AdjustInventoryRequestDto { QuantityChange = 5 };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdForInventoryAsync(404))
                .ReturnsAsync((Product?)null);

            var action = async () =>
                await _productService.AdjustInventoryAsync(404, request, userId: 1);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found.");

            _productRepositoryMock.Verify(
                repo => repo.AddInventoryTransactionAsync(It.IsAny<InventoryTransaction>()),
                Times.Never
            );
            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AdjustInventoryAsync_ShouldThrowBadRequestException_WhenResultingQuantityIsNegative()
        {
            var product = new Product { Id = 1, QuantityOnHand = 3 };
            var request = new AdjustInventoryRequestDto { QuantityChange = -5 };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdForInventoryAsync(1))
                .ReturnsAsync(product);

            var action = async () =>
                await _productService.AdjustInventoryAsync(1, request, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Inventory quantity cannot be negative.");

            _productRepositoryMock.Verify(
                repo => repo.AddInventoryTransactionAsync(It.IsAny<InventoryTransaction>()),
                Times.Never
            );
            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AdjustInventoryAsync_ShouldThrowBadRequestException_WhenQuantityChangeIsZero()
        {
            var product = new Product { Id = 1, QuantityOnHand = 5 };
            var request = new AdjustInventoryRequestDto { QuantityChange = 0 };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdForInventoryAsync(1))
                .ReturnsAsync(product);

            var action = async () =>
                await _productService.AdjustInventoryAsync(1, request, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Quantity change cannot be zero.");

            _productRepositoryMock.Verify(
                repo => repo.AddInventoryTransactionAsync(It.IsAny<InventoryTransaction>()),
                Times.Never
            );
            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AdjustInventoryAsync_ShouldAdjustQuantityAndRecordTransaction_WhenValid()
        {
            var product = new Product
            {
                Id = 1,
                Name = "Desk",
                Sku = "SKU-DESK",
                CategoryId = 1,
                QuantityOnHand = 10,
                Version = 1,
            };

            var request = new AdjustInventoryRequestDto
            {
                QuantityChange = 5,
                Notes = " Restocked ",
            };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdForInventoryAsync(1))
                .ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repo => repo.AddInventoryTransactionAsync(It.IsAny<InventoryTransaction>()))
                .Returns(Task.CompletedTask);

            _productRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _productService.AdjustInventoryAsync(1, request, userId: 8);

            result.Should().NotBeNull();
            result.QuantityOnHand.Should().Be(15);

            product.QuantityOnHand.Should().Be(15);
            product.Version.Should().Be(2);
            product.UpdatedBy.Should().Be(8);
            product.UpdatedAt.Should().NotBeNull();

            _productRepositoryMock.Verify(
                repo =>
                    repo.AddInventoryTransactionAsync(
                        It.Is<InventoryTransaction>(t =>
                            t.ProductId == 1
                            && t.ChangeType == InventoryChangeType.ManualAdjustment
                            && t.QuantityBefore == 10
                            && t.QuantityChange == 5
                            && t.QuantityAfter == 15
                            && t.Notes == "Restocked"
                            && t.CreatedBy == 8
                        )
                    ),
                Times.Once
            );

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateProductAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
        {
            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(404))
                .ReturnsAsync((Product?)null);

            var action = async () => await _productService.DeactivateProductAsync(404, userId: 1);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found.");

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeactivateProductAsync_ShouldThrowBadRequestException_WhenProductIsAlreadyInactive()
        {
            var product = new Product { Id = 1, IsActive = false };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            var action = async () => await _productService.DeactivateProductAsync(1, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Product is already inactive.");

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeactivateProductAsync_ShouldDeactivateProductAndIncrementVersion_WhenActive()
        {
            var product = new Product
            {
                Id = 1,
                Name = "Headset",
                Sku = "SKU-HEAD",
                IsActive = true,
                Version = 1,
            };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _productService.DeactivateProductAsync(1, userId: 3);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.IsActive.Should().BeFalse();
            result.UpdatedBy.Should().Be(3);

            product.IsActive.Should().BeFalse();
            product.Version.Should().Be(2);
            product.UpdatedBy.Should().Be(3);
            product.UpdatedAt.Should().NotBeNull();

            _productRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldReturnPagedResponse_WhenCalled()
        {
            var request = new GetProductsRequestDto { Page = 1 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("10");

            var productList = new List<ProductListResponseDto>
            {
                new ProductListResponseDto
                {
                    Id = 1,
                    Name = "Item A",
                    Sku = "SKU-A",
                },
            };

            _productRepositoryMock
                .Setup(repo => repo.GetProductsAsync(request, 10))
                .ReturnsAsync((productList, 25));

            var result = await _productService.GetProductsAsync(request);

            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(25);
            result.TotalPages.Should().Be(3);
            result.Data.Should().HaveCount(1);

            _productRepositoryMock.Verify(repo => repo.GetProductsAsync(request, 10), Times.Once);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldDefaultPageSizeTo10_WhenConfigurationIsZeroOrNegative()
        {
            var request = new GetProductsRequestDto { Page = 1 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("0");

            _productRepositoryMock
                .Setup(repo => repo.GetProductsAsync(request, 10))
                .ReturnsAsync((new List<ProductListResponseDto>(), 0));

            var result = await _productService.GetProductsAsync(request);

            result.PageSize.Should().Be(10);

            _productRepositoryMock.Verify(repo => repo.GetProductsAsync(request, 10), Times.Once);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldDefaultPageToOne_WhenRequestedPageIsLessThanOne()
        {
            var request = new GetProductsRequestDto { Page = -3 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("10");

            _productRepositoryMock
                .Setup(repo => repo.GetProductsAsync(request, 10))
                .ReturnsAsync((new List<ProductListResponseDto>(), 0));

            var result = await _productService.GetProductsAsync(request);

            request.Page.Should().Be(1);
            result.Page.Should().Be(1);

            _productRepositoryMock.Verify(repo => repo.GetProductsAsync(request, 10), Times.Once);
        }
    }
}
