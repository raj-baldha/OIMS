using System;
using System.Collections.Generic;
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
    public class InventoryControllerTests
    {
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly InventoryController _controller;

        public InventoryControllerTests()
        {
            _inventoryServiceMock = new Mock<IInventoryService>();
            _controller = new InventoryController(_inventoryServiceMock.Object);
        }

        [Fact]
        public async Task GetLowStockProducts_ShouldReturnOkWithProductList()
        {
            var lowStockList = new List<LowStockProductResponseDto>
            {
                new LowStockProductResponseDto
                {
                    Id = 1,
                    Name = "Mechanical Keyboard",
                    Sku = "SKU-KEY-01",
                    CategoryId = 2,
                    CategoryName = "Peripherals",
                    QuantityOnHand = 2,
                    MinStockLevel = 10,
                    Shortage = 8,
                    IsActive = true,
                },
            };

            _inventoryServiceMock
                .Setup(s => s.GetLowStockProductsAsync())
                .ReturnsAsync(lowStockList);

            var actionResult = await _controller.GetLowStockProducts();

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<List<LowStockProductResponseDto>>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(lowStockList);
            apiResponse.Message.Should().Be("Low-stock products retrieved successfully.");

            _inventoryServiceMock.Verify(s => s.GetLowStockProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetInventoryHistory_ShouldReturnOkWithPagedHistory_WhenProductExists()
        {
            int productId = 5;
            var request = new GetInventoryHistoryRequestDto { Page = 1 };

            var pagedResponse = new PagedResponseDto<InventoryHistoryResponseDto>
            {
                Data = new List<InventoryHistoryResponseDto>
                {
                    new InventoryHistoryResponseDto
                    {
                        Id = 101,
                        ProductId = productId,
                        ChangeType = "ManualAdjustment",
                        QuantityBefore = 10,
                        QuantityChange = 5,
                        QuantityAfter = 15,
                        Notes = "Restock",
                        CreatedBy = 1,
                        CreatedByName = "Admin",
                        CreatedAt = DateTime.UtcNow,
                    },
                },
                Page = 1,
                PageSize = 10,
                TotalRecords = 1,
                TotalPages = 1,
            };

            _inventoryServiceMock
                .Setup(s => s.GetInventoryHistoryAsync(productId, request))
                .ReturnsAsync(pagedResponse);

            var actionResult = await _controller.GetInventoryHistory(productId, request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse =
                okResult.Value as ApiResponse<PagedResponseDto<InventoryHistoryResponseDto>>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(pagedResponse);
            apiResponse.Message.Should().Be("Inventory history retrieved successfully.");

            _inventoryServiceMock.Verify(
                s => s.GetInventoryHistoryAsync(productId, request),
                Times.Once
            );
        }
    }
}
