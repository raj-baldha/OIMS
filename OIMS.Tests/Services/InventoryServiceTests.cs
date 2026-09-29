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
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly InventoryService _inventoryService;

        public InventoryServiceTests()
        {
            _inventoryRepositoryMock = new Mock<IInventoryRepository>();
            _configurationMock = new Mock<IConfiguration>();

            _inventoryService = new InventoryService(
                _inventoryRepositoryMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnListFromRepository()
        {
            var expectedList = new List<LowStockProductResponseDto>
            {
                new LowStockProductResponseDto
                {
                    Id = 1,
                    Name = "Keyboard",
                    Sku = "SKU-KEY",
                    QuantityOnHand = 2,
                    MinStockLevel = 10,
                    Shortage = 8,
                    IsActive = true,
                },
            };

            _inventoryRepositoryMock
                .Setup(repo => repo.GetLowStockProductsAsync())
                .ReturnsAsync(expectedList);

            var result = await _inventoryService.GetLowStockProductsAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Keyboard");
            result[0].Shortage.Should().Be(8);

            _inventoryRepositoryMock.Verify(repo => repo.GetLowStockProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldThrowNotFoundException_WhenProductDoesNotExist()
        {
            int productId = 999;
            var request = new GetInventoryHistoryRequestDto { Page = 1 };

            _inventoryRepositoryMock
                .Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(false);

            var action = async () =>
                await _inventoryService.GetInventoryHistoryAsync(productId, request);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found.");

            _inventoryRepositoryMock.Verify(
                repo =>
                    repo.GetInventoryHistoryAsync(
                        It.IsAny<int>(),
                        It.IsAny<GetInventoryHistoryRequestDto>(),
                        It.IsAny<int>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldReturnPagedResponse_WhenProductExists()
        {
            int productId = 1;
            var request = new GetInventoryHistoryRequestDto { Page = 1 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("5");

            _inventoryRepositoryMock
                .Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(true);

            var historyItems = new List<InventoryHistoryResponseDto>
            {
                new InventoryHistoryResponseDto
                {
                    Id = 101,
                    ProductId = productId,
                    ChangeType = "StockIn",
                    QuantityBefore = 5,
                    QuantityChange = 10,
                    QuantityAfter = 15,
                },
            };

            _inventoryRepositoryMock
                .Setup(repo => repo.GetInventoryHistoryAsync(productId, request, 5))
                .ReturnsAsync((historyItems, 12));

            var result = await _inventoryService.GetInventoryHistoryAsync(productId, request);

            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(5);
            result.TotalRecords.Should().Be(12);
            result.TotalPages.Should().Be(3);
            result.Data.Should().HaveCount(1);
            result.Data[0].QuantityAfter.Should().Be(15);
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldSetDefaultPageSizeTo10_WhenConfiguredPageSizeIsZeroOrNegative()
        {
            int productId = 2;
            var request = new GetInventoryHistoryRequestDto { Page = 1 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("0");

            _inventoryRepositoryMock
                .Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(true);

            _inventoryRepositoryMock
                .Setup(repo => repo.GetInventoryHistoryAsync(productId, request, 10))
                .ReturnsAsync((new List<InventoryHistoryResponseDto>(), 0));

            var result = await _inventoryService.GetInventoryHistoryAsync(productId, request);

            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(0);

            _inventoryRepositoryMock.Verify(
                repo => repo.GetInventoryHistoryAsync(productId, request, 10),
                Times.Once
            );
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldDefaultPageToOne_WhenRequestedPageIsLessThanOne()
        {
            int productId = 3;
            var request = new GetInventoryHistoryRequestDto { Page = -5 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("10");

            _inventoryRepositoryMock
                .Setup(repo => repo.ProductExistsAsync(productId))
                .ReturnsAsync(true);

            _inventoryRepositoryMock
                .Setup(repo => repo.GetInventoryHistoryAsync(productId, request, 10))
                .ReturnsAsync((new List<InventoryHistoryResponseDto>(), 0));

            var result = await _inventoryService.GetInventoryHistoryAsync(productId, request);

            request.Page.Should().Be(1);
            result.Page.Should().Be(1);

            _inventoryRepositoryMock.Verify(
                repo => repo.GetInventoryHistoryAsync(productId, request, 10),
                Times.Once
            );
        }
    }
}
