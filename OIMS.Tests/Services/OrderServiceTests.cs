using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using OIMS.Application.DTOs.Orders;
using OIMS.Application.DTOs.Request;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _configurationMock = new Mock<IConfiguration>();

            _orderService = new OrderService(
                _orderRepositoryMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
        {
            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Street",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2 },
                },
            };

            _orderRepositoryMock
                .Setup(repo => repo.GetCustomerAsync(10))
                .ReturnsAsync((Customer?)null);

            var action = async () => await _orderService.CreateOrderAsync(request, 10);

            await action
                .Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Customer not found or inactive.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowBadRequestException_WhenItemsListIsEmpty()
        {
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Street",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>(),
            };

            _orderRepositoryMock.Setup(repo => repo.GetCustomerAsync(1)).ReturnsAsync(customer);

            var action = async () => await _orderService.CreateOrderAsync(request, 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Order must contain at least one product.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowBadRequestException_WhenShippingAddressIsBlank()
        {
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "   ",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2 },
                },
            };

            _orderRepositoryMock.Setup(repo => repo.GetCustomerAsync(1)).ReturnsAsync(customer);

            var action = async () => await _orderService.CreateOrderAsync(request, 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Shipping address is required.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowBadRequestException_WhenPaymentMethodIsInvalid()
        {
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Street",
                PaymentMethod = "CryptoCurrency",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2 },
                },
            };

            _orderRepositoryMock.Setup(repo => repo.GetCustomerAsync(1)).ReturnsAsync(customer);

            var action = async () => await _orderService.CreateOrderAsync(request, 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Invalid payment method.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowBadRequestException_WhenDuplicateProductIdsExist()
        {
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Street",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2 },
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 3 },
                },
            };

            _orderRepositoryMock.Setup(repo => repo.GetCustomerAsync(1)).ReturnsAsync(customer);

            var action = async () => await _orderService.CreateOrderAsync(request, 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("The same product cannot be added multiple times.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowNotFoundException_WhenAnyProductDoesNotExist()
        {
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Street",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 2 },
                    new CreateOrderItemRequestDto { ProductId = 2, Quantity = 1 },
                },
            };

            _orderRepositoryMock.Setup(repo => repo.GetCustomerAsync(1)).ReturnsAsync(customer);

            _orderRepositoryMock
                .Setup(repo => repo.GetProductsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(
                    new List<Product>
                    {
                        new Product
                        {
                            Id = 1,
                            Name = "Item 1",
                            SellingPrice = 50,
                            QuantityOnHand = 10,
                        },
                    }
                );

            var action = async () => await _orderService.CreateOrderAsync(request, 1);

            await action
                .Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("One or more products were not found or are inactive.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowBadRequestException_WhenItemQuantityIsZeroOrNegative()
        {
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Street",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 0 },
                },
            };

            _orderRepositoryMock.Setup(repo => repo.GetCustomerAsync(1)).ReturnsAsync(customer);

            _orderRepositoryMock
                .Setup(repo => repo.GetProductsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(
                    new List<Product>
                    {
                        new Product
                        {
                            Id = 1,
                            Name = "Item 1",
                            SellingPrice = 50,
                            QuantityOnHand = 10,
                        },
                    }
                );

            var action = async () => await _orderService.CreateOrderAsync(request, 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Product quantity must be greater than zero.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowBadRequestException_WhenStockIsInsufficient()
        {
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "123 Street",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 1, Quantity = 10 },
                },
            };

            _orderRepositoryMock.Setup(repo => repo.GetCustomerAsync(1)).ReturnsAsync(customer);

            _orderRepositoryMock
                .Setup(repo => repo.GetProductsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(
                    new List<Product>
                    {
                        new Product
                        {
                            Id = 1,
                            Name = "Gaming Laptop",
                            SellingPrice = 1000,
                            QuantityOnHand = 3,
                        },
                    }
                );

            var action = async () => await _orderService.CreateOrderAsync(request, 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Insufficient stock for product 'Gaming Laptop'.");
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
        {
            var request = new UpdateOrderStatusRequestDto { Status = "Confirmed" };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(404))
                .ReturnsAsync((Order?)null);

            var action = async () =>
                await _orderService.UpdateOrderStatusAsync(404, request, userId: 1);

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Order not found.");
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldThrowBadRequestException_WhenStatusStringIsInvalid()
        {
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var request = new UpdateOrderStatusRequestDto { Status = "UnknownStatus" };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(1))
                .ReturnsAsync(order);

            var action = async () =>
                await _orderService.UpdateOrderStatusAsync(1, request, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Invalid order status.");
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldThrowBadRequestException_WhenStatusTransitionIsInvalid()
        {
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var request = new UpdateOrderStatusRequestDto { Status = "Delivered" };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(1))
                .ReturnsAsync(order);

            var action = async () =>
                await _orderService.UpdateOrderStatusAsync(1, request, userId: 1);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Invalid order status transition from Pending to Delivered.");
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldUpdateStatusAndAddHistory_WhenTransitionIsValid()
        {
            var order = new Order { Id = 1, Status = OrderStatus.Pending };
            var request = new UpdateOrderStatusRequestDto { Status = "Confirmed" };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(1))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(repo => repo.AddOrderStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _orderService.UpdateOrderStatusAsync(1, request, userId: 5);

            order.Status.Should().Be(OrderStatus.Confirmed);
            order.UpdatedBy.Should().Be(5);
            order.UpdatedAt.Should().NotBeNull();

            _orderRepositoryMock.Verify(
                repo =>
                    repo.AddOrderStatusHistoryAsync(
                        It.Is<OrderStatusHistory>(h =>
                            h.OrderId == 1 && h.Status == OrderStatus.Confirmed && h.ChangedBy == 5
                        )
                    ),
                Times.Once
            );

            _orderRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
        {
            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(404))
                .ReturnsAsync((Order?)null);

            var action = async () =>
                await _orderService.CancelOrderAsync(404, userId: 1, role: "Admin");

            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Order not found.");
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldThrowForbiddenException_WhenCustomerCancelsSomeoneElsesOrder()
        {
            var order = new Order
            {
                Id = 1,
                CustomerId = 10,
                Status = OrderStatus.Pending,
            };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(1))
                .ReturnsAsync(order);

            var action = async () =>
                await _orderService.CancelOrderAsync(1, userId: 99, role: "Customer");

            await action
                .Should()
                .ThrowAsync<ForbiddenException>()
                .WithMessage("You are not allowed to cancel this order.");
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldThrowBadRequestException_WhenOrderStatusIsNotPendingOrConfirmed()
        {
            var order = new Order
            {
                Id = 1,
                CustomerId = 10,
                Status = OrderStatus.Shipped,
            };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(1))
                .ReturnsAsync(order);

            var action = async () =>
                await _orderService.CancelOrderAsync(1, userId: 10, role: "Customer");

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage("Order cannot be cancelled at its current status.");
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldCancelOrderAndSaveHistory_WhenOrderIsPending()
        {
            var order = new Order
            {
                Id = 1,
                CustomerId = 10,
                Status = OrderStatus.Pending,
            };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrderForStatusUpdateAsync(1))
                .ReturnsAsync(order);

            _orderRepositoryMock
                .Setup(repo => repo.AddOrderStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _orderService.CancelOrderAsync(1, userId: 10, role: "Customer");

            order.Status.Should().Be(OrderStatus.Cancelled);
            order.UpdatedBy.Should().Be(10);
            order.UpdatedAt.Should().NotBeNull();

            _orderRepositoryMock.Verify(
                repo =>
                    repo.AddOrderStatusHistoryAsync(
                        It.Is<OrderStatusHistory>(h =>
                            h.OrderId == 1 && h.Status == OrderStatus.Cancelled && h.ChangedBy == 10
                        )
                    ),
                Times.Once
            );

            _orderRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldPassUserId_WhenRoleIsCustomer()
        {
            var request = new GetOrdersRequestDto { Page = 1 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("10");

            var orderList = new List<OrderListResponseDto>
            {
                new OrderListResponseDto
                {
                    Id = 1,
                    CustomerId = 20,
                    TotalAmount = 150,
                },
            };

            _orderRepositoryMock
                .Setup(repo => repo.GetOrdersAsync(request, 10, 20))
                .ReturnsAsync((orderList, 1));

            var result = await _orderService.GetOrdersAsync(request, userId: 20, role: "Customer");

            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(1);
            result.TotalPages.Should().Be(1);

            _orderRepositoryMock.Verify(repo => repo.GetOrdersAsync(request, 10, 20), Times.Once);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldPassNullCustomerId_WhenRoleIsAdmin()
        {
            var request = new GetOrdersRequestDto { Page = 0 };

            _configurationMock.Setup(cfg => cfg["Pagination:PageSize"]).Returns("5");

            _orderRepositoryMock
                .Setup(repo => repo.GetOrdersAsync(request, 5, null))
                .ReturnsAsync((new List<OrderListResponseDto>(), 12));

            var result = await _orderService.GetOrdersAsync(request, userId: 1, role: "Admin");

            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(5);
            result.TotalRecords.Should().Be(12);
            result.TotalPages.Should().Be(3);

            _orderRepositoryMock.Verify(repo => repo.GetOrdersAsync(request, 5, null), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldSucceedAndExecuteAllProjections_WhenDataIsValid()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            int customerId = 1;
            var customer = new Customer
            {
                Id = customerId,
                FirstName = "Clark",
                LastName = "Kent",
                Email = "clark@dailyplanet.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "344 Clinton St",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 10, Quantity = 2 },
                },
            };

            var product = new Product
            {
                Id = 10,
                Name = "Desk Lamp",
                SellingPrice = 25m,
                QuantityOnHand = 10,
                Version = 1,
            };

            _orderRepositoryMock.Setup(r => r.GetCustomerAsync(customerId)).ReturnsAsync(customer);

            _orderRepositoryMock
                .Setup(r => r.GetProductsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Product> { product });

            _orderRepositoryMock.Setup(r => r.BeginTransactionAsync()).Returns(Task.CompletedTask);

            _orderRepositoryMock
                .Setup(r => r.AddOrderAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock
                .Setup(r => r.AddInventoryTransactionsAsync(It.IsAny<List<InventoryTransaction>>()))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock
                .Setup(r => r.AddOrderDocumentAsync(It.IsAny<OrderDocument>()))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock
                .Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>()))
                .Returns(Task.CompletedTask);

            _orderRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            _orderRepositoryMock.Setup(r => r.CommitTransactionAsync()).Returns(Task.CompletedTask);

            var result = await _orderService.CreateOrderAsync(request, customerId);

            result.Should().NotBeNull();
            result.CustomerId.Should().Be(customerId);
            result.Subtotal.Should().Be(50m);
            result.TotalAmount.Should().Be(50m);
            result.Items.Should().HaveCount(1);
            result.Items[0].ProductId.Should().Be(10);
            result.Items[0].ProductName.Should().Be("Desk Lamp");
            result.Items[0].Quantity.Should().Be(2);
            result.Items[0].UnitPrice.Should().Be(25m);
            result.Items[0].LineTotal.Should().Be(50m);

            product.QuantityOnHand.Should().Be(8);
            product.Version.Should().Be(2);

            _orderRepositoryMock.Verify(r => r.CommitTransactionAsync(), Times.Once);
            _orderRepositoryMock.Verify(r => r.RollbackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldRollbackAndRethrow_WhenRepositoryFails()
        {
            int customerId = 1;
            var customer = new Customer
            {
                Id = customerId,
                FirstName = "Diana",
                LastName = "Prince",
                Email = "diana@themyscira.com",
            };

            var request = new CreateOrderRequestDto
            {
                ShippingAddress = "Gateway City",
                PaymentMethod = "Card",
                Items = new List<CreateOrderItemRequestDto>
                {
                    new CreateOrderItemRequestDto { ProductId = 20, Quantity = 1 },
                },
            };

            var product = new Product
            {
                Id = 20,
                Name = "Shield",
                SellingPrice = 100m,
                QuantityOnHand = 5,
                Version = 1,
            };

            _orderRepositoryMock.Setup(r => r.GetCustomerAsync(customerId)).ReturnsAsync(customer);

            _orderRepositoryMock
                .Setup(r => r.GetProductsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<Product> { product });

            _orderRepositoryMock.Setup(r => r.BeginTransactionAsync()).Returns(Task.CompletedTask);

            _orderRepositoryMock
                .Setup(r => r.AddOrderAsync(It.IsAny<Order>()))
                .ThrowsAsync(new InvalidOperationException("DB save failed"));

            _orderRepositoryMock
                .Setup(r => r.RollbackTransactionAsync())
                .Returns(Task.CompletedTask);

            var action = async () => await _orderService.CreateOrderAsync(request, customerId);

            await action
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("DB save failed");

            _orderRepositoryMock.Verify(r => r.RollbackTransactionAsync(), Times.Once);
            _orderRepositoryMock.Verify(r => r.CommitTransactionAsync(), Times.Never);
        }
    }
}
