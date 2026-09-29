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
using OIMS.Application.DTOs.Orders;
using OIMS.Application.DTOs.Request;
using OIMS.Application.Interfaces.Services;
using Xunit;

namespace OIMS.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _controller = new OrdersController(_orderServiceMock.Object);
        }

        private void SetupUserClaims(int userId, string role = "Customer")
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
        public async Task CreateOrder_ShouldReturn201Created_WhenOrderPlacedSuccessfully()
        {
            int customerId = 5;
            SetupUserClaims(customerId, "Customer");

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Market Street",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2 },
                },
            };

            var createdOrderResponse = new CreatedOrderResponseDto
            {
                Id = 101,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                ShippingAddress = "123 Market Street",
                Subtotal = 100m,
                DiscountAmount = 0m,
                TotalAmount = 100m,
                PaymentMethod = "Card",
                PaymentStatus = "Pending",
            };

            _orderServiceMock
                .Setup(s => s.CreateOrderAsync(request, customerId))
                .ReturnsAsync(createdOrderResponse);

            var actionResult = await _controller.CreateOrder(request);

            var objectResult = actionResult as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(StatusCodes.Status201Created);

            var apiResponse = objectResult.Value as ApiResponse<CreatedOrderResponseDto>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(createdOrderResponse);
            apiResponse.Message.Should().Be("Order placed successfully.");

            _orderServiceMock.Verify(s => s.CreateOrderAsync(request, customerId), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnOk_WhenStatusUpdatedSuccessfully()
        {
            int orderId = 101;
            int userId = 10;
            SetupUserClaims(userId, "Manager");

            var request = new UpdateOrderStatusRequestDto { Status = "Confirmed" };

            _orderServiceMock
                .Setup(s => s.UpdateOrderStatusAsync(orderId, request, userId))
                .Returns(Task.CompletedTask);

            var actionResult = await _controller.UpdateOrderStatus(orderId, request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Message.Should().Be("Order status updated successfully.");

            _orderServiceMock.Verify(
                s => s.UpdateOrderStatusAsync(orderId, request, userId),
                Times.Once
            );
        }

        [Fact]
        public async Task CancelOrder_ShouldReturnOk_WhenOrderCancelledSuccessfully()
        {
            int orderId = 101;
            int userId = 5;
            string role = "Customer";
            SetupUserClaims(userId, role);

            _orderServiceMock
                .Setup(s => s.CancelOrderAsync(orderId, userId, role))
                .Returns(Task.CompletedTask);

            var actionResult = await _controller.CancelOrder(orderId);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<object>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Message.Should().Be("Order cancelled successfully.");

            _orderServiceMock.Verify(s => s.CancelOrderAsync(orderId, userId, role), Times.Once);
        }

        [Fact]
        public async Task GetOrders_ShouldReturnOkWithPagedOrders_WhenCalled()
        {
            int userId = 5;
            string role = "Customer";
            SetupUserClaims(userId, role);

            var request = new GetOrdersRequestDto
            {
                Page = 1,
                SortBy = "orderDate",
                SortOrder = "desc",
            };

            var pagedResponse = new PagedResponseDto<OrderListResponseDto>
            {
                Data = new List<OrderListResponseDto>
                {
                    new OrderListResponseDto
                    {
                        Id = 101,
                        CustomerId = userId,
                        CustomerName = "John Doe",
                        OrderDate = DateTime.UtcNow,
                        Status = "Pending",
                        TotalAmount = 150m,
                    },
                },
                Page = 1,
                PageSize = 10,
                TotalRecords = 1,
                TotalPages = 1,
            };

            _orderServiceMock
                .Setup(s => s.GetOrdersAsync(request, userId, role))
                .ReturnsAsync(pagedResponse);

            var actionResult = await _controller.GetOrders(request);

            var okResult = actionResult as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

            var apiResponse = okResult.Value as ApiResponse<PagedResponseDto<OrderListResponseDto>>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Data.Should().BeEquivalentTo(pagedResponse);
            apiResponse.Message.Should().Be("Orders retrieved successfully.");

            _orderServiceMock.Verify(s => s.GetOrdersAsync(request, userId, role), Times.Once);
        }
    }
}
