using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OIMS.Application.DTOs.Orders;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;
using OIMS.Infrastructure.Repositories;
using Xunit;

namespace OIMS.Tests.Repository
{
    public class OrderRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private Customer CreateCustomer(
            int id,
            string email = "customer@test.com",
            bool isActive = true,
            bool isDeleted = false
        )
        {
            return new Customer
            {
                Id = id,
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "hashed",
                IsActive = isActive,
                IsDeleted = isDeleted,
            };
        }

        private Product CreateProduct(
            int id,
            string name = "Sample Product",
            string sku = "SKU-1",
            bool isActive = true,
            bool isDeleted = false
        )
        {
            return new Product
            {
                Id = id,
                Name = name,
                Sku = sku,
                CategoryId = 1,
                SellingPrice = 100,
                CostPrice = 70,
                IsActive = isActive,
                IsDeleted = isDeleted,
            };
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnCustomer_WhenCustomerExistsAndActive()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(1);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetCustomerAsync(1);

            result.Should().NotBeNull();
            result!.Email.Should().Be("customer@test.com");
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnNull_WhenCustomerIsInactive()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(1, isActive: false);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetCustomerAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnNull_WhenCustomerIsDeleted()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(1, isDeleted: true);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetCustomerAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCustomerAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var result = await repository.GetCustomerAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProductsAsync_ShouldReturnOnlyActiveAndNonDeletedProducts()
        {
            using var context = CreateDbContext();

            var p1 = CreateProduct(1, "Prod1", "SKU-1", isActive: true, isDeleted: false);
            var p2 = CreateProduct(2, "Prod2", "SKU-2", isActive: false, isDeleted: false);
            var p3 = CreateProduct(3, "Prod3", "SKU-3", isActive: true, isDeleted: true);
            var p4 = CreateProduct(4, "Prod4", "SKU-4", isActive: true, isDeleted: false);

            context.Products.AddRange(p1, p2, p3, p4);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetProductsAsync(new List<int> { 1, 2, 3 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task AddOrderAsync_ShouldAddOrderToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var order = new Order
            {
                Id = 1,
                CustomerId = 10,
                ShippingAddress = "123 Street",
                Subtotal = 100,
                DiscountAmount = 10,
                TotalAmount = 90,
                Status = OrderStatus.Pending,
            };

            await repository.AddOrderAsync(order);
            await repository.SaveChangesAsync();

            var savedOrder = await context.Orders.FirstOrDefaultAsync(x => x.Id == 1);

            savedOrder.Should().NotBeNull();
            savedOrder!.TotalAmount.Should().Be(90);
        }

        [Fact]
        public async Task AddInventoryTransactionsAsync_ShouldAddAllTransactions()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var transactions = new List<InventoryTransaction>
            {
                new InventoryTransaction
                {
                    ProductId = 1,
                    ChangeType = InventoryChangeType.ManualAdjustment,
                    QuantityBefore = 10,
                    QuantityChange = 5,
                    QuantityAfter = 15,
                },
                new InventoryTransaction
                {
                    ProductId = 2,
                    ChangeType = InventoryChangeType.OrderPlacement,
                    QuantityBefore = 20,
                    QuantityChange = -5,
                    QuantityAfter = 15,
                },
            };

            await repository.AddInventoryTransactionsAsync(transactions);
            await repository.SaveChangesAsync();

            var count = await context.InventoryTransactions.CountAsync();
            count.Should().Be(2);
        }

        [Fact]
        public async Task AddAuditLogAsync_ShouldAddAuditLogToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var auditLog = new AuditLog
            {
                Action = "CREATE",
                EntityName = "Order",
                EntityId = 1,
            };

            await repository.AddAuditLogAsync(auditLog);
            await repository.SaveChangesAsync();

            var savedLog = await context.AuditLogs.FirstOrDefaultAsync(x => x.EntityId == 1);
            savedLog.Should().NotBeNull();
            savedLog!.Action.Should().Be("CREATE");
        }

        [Fact]
        public async Task AddOrderStatusHistoryAsync_ShouldAddStatusHistoryToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var history = new OrderStatusHistory { OrderId = 1, Status = OrderStatus.Confirmed };

            await repository.AddOrderStatusHistoryAsync(history);
            await repository.SaveChangesAsync();

            var savedHistory = await context.OrderStatusHistories.FirstOrDefaultAsync(x =>
                x.OrderId == 1
            );
            savedHistory.Should().NotBeNull();
            savedHistory!.Status.Should().Be(OrderStatus.Confirmed);
        }

        [Fact]
        public async Task AddOrderDocumentAsync_ShouldAddDocumentToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var document = new OrderDocument
            {
                OrderId = 1,
                FileUrl = "/docs/invoice1.pdf",
                StoredFileName = "invoice1.pdf",
                FileSize = 1024,
            };

            await repository.AddOrderDocumentAsync(document);
            await repository.SaveChangesAsync();

            var savedDoc = await context.OrderDocuments.FirstOrDefaultAsync(x => x.OrderId == 1);
            savedDoc.Should().NotBeNull();
            savedDoc!.FileUrl.Should().Be("/docs/invoice1.pdf");
        }

        [Fact]
        public async Task GetOrderForStatusUpdateAsync_ShouldReturnOrderWithItemsAndPayment()
        {
            using var context = CreateDbContext();

            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                ShippingAddress = "Main Road",
                Subtotal = 100,
                TotalAmount = 100,
                Status = OrderStatus.Pending,
                IsDeleted = false,
                Payment = new Payment
                {
                    Id = 1,
                    OrderId = 1,
                    Amount = 100,
                    PaymentMethod = PaymentMethod.Card,
                    PaymentStatus = PaymentStatus.Paid,
                },
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = 1,
                        Quantity = 1,
                        UnitPrice = 100,
                        LineTotal = 100,
                    },
                },
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetOrderForStatusUpdateAsync(1);

            result.Should().NotBeNull();
            result!.Payment.Should().NotBeNull();
            result.OrderItems.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetOrderForStatusUpdateAsync_ShouldReturnNull_WhenOrderIsDeleted()
        {
            using var context = CreateDbContext();

            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                ShippingAddress = "Main Road",
                Subtotal = 100,
                TotalAmount = 100,
                IsDeleted = true,
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetOrderForStatusUpdateAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetOrderForInvoiceAsync_ShouldReturnCompleteOrder()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "invoice@test.com");
            var product = CreateProduct(1, "Monitor", "SKU-M");

            context.Customers.Add(customer);
            context.Products.Add(product);

            var order = new Order
            {
                Id = 10,
                CustomerId = 1,
                Customer = customer,
                ShippingAddress = "Street 10",
                Subtotal = 200,
                TotalAmount = 200,
                IsDeleted = false,
                Payment = new Payment
                {
                    Id = 1,
                    OrderId = 10,
                    Amount = 200,
                    PaymentMethod = PaymentMethod.NetBanking,
                    PaymentStatus = PaymentStatus.Paid,
                },
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        OrderId = 10,
                        ProductId = 1,
                        Product = product,
                        Quantity = 1,
                        UnitPrice = 200,
                        LineTotal = 200,
                    },
                },
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetOrderForInvoiceAsync(10);

            result.Should().NotBeNull();
            result!.Customer.Should().NotBeNull();
            result.Payment.Should().NotBeNull();
            result.OrderItems.Should().HaveCount(1);
            result.OrderItems.First().Product.Should().NotBeNull();
        }

        [Fact]
        public async Task GetOrderForInvoiceAsync_ShouldReturnNull_WhenOrderIsDeleted()
        {
            using var context = CreateDbContext();

            var order = new Order
            {
                Id = 10,
                CustomerId = 1,
                ShippingAddress = "Street 10",
                Subtotal = 200,
                TotalAmount = 200,
                IsDeleted = true,
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetOrderForInvoiceAsync(10);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CommitTransactionAsync_ShouldExecuteSafely_WhenNoTransactionExists()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var act = async () => await repository.CommitTransactionAsync();

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RollbackTransactionAsync_ShouldExecuteSafely_WhenNoTransactionExists()
        {
            using var context = CreateDbContext();
            var repository = new OrderRepository(context);

            var act = async () => await repository.RollbackTransactionAsync();

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldFilterByCustomerId()
        {
            using var context = CreateDbContext();

            var customer1 = CreateCustomer(1, "cust1@test.com");
            var customer2 = CreateCustomer(2, "cust2@test.com");
            context.Customers.AddRange(customer1, customer2);

            var o1 = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer1,
                ShippingAddress = "A",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };
            var o2 = new Order
            {
                Id = 2,
                CustomerId = 2,
                Customer = customer2,
                ShippingAddress = "B",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };
            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var request = new GetOrdersRequestDto { Page = 1 };

            var (data, totalRecords) = await repository.GetOrdersAsync(request, 10, customerId: 1);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].CustomerId.Should().Be(1);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldFilterByNumericSearch_MatchingOrderId()
        {
            using var context = CreateDbContext();

            var customer1 = CreateCustomer(1, "cust1@test.com");
            customer1.FirstName = "John";
            customer1.LastName = "Doe";
            context.Customers.Add(customer1);

            var o1 = new Order
            {
                Id = 101,
                CustomerId = 1,
                Customer = customer1,
                ShippingAddress = "A",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };

            var o2 = new Order
            {
                Id = 202,
                CustomerId = 1,
                Customer = customer1,
                ShippingAddress = "B",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };

            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var request = new GetOrdersRequestDto { Search = "101", Page = 1 };

            var (data, totalRecords) = await repository.GetOrdersAsync(
                request,
                10,
                customerId: null
            );

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Id.Should().Be(101);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldFilterByTextSearch_MatchingCustomerName()
        {
            using var context = CreateDbContext();

            var c1 = CreateCustomer(1, "alex@test.com");
            c1.FirstName = "Alex";
            var c2 = CreateCustomer(2, "bob@test.com");
            c2.FirstName = "Bob";
            context.Customers.AddRange(c1, c2);

            var o1 = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = c1,
                ShippingAddress = "A",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };
            var o2 = new Order
            {
                Id = 2,
                CustomerId = 2,
                Customer = c2,
                ShippingAddress = "B",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };
            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var request = new GetOrdersRequestDto { Search = "Alex", Page = 1 };

            var (data, totalRecords) = await repository.GetOrdersAsync(
                request,
                10,
                customerId: null
            );

            totalRecords.Should().Be(1);
            data[0].CustomerName.Should().Contain("Alex");
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldFilterByStatus()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "status@test.com");
            context.Customers.Add(customer);

            var o1 = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                Status = OrderStatus.Shipped,
                ShippingAddress = "A",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };
            var o2 = new Order
            {
                Id = 2,
                CustomerId = 1,
                Customer = customer,
                Status = OrderStatus.Cancelled,
                ShippingAddress = "B",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };
            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var request = new GetOrdersRequestDto { Status = "Shipped", Page = 1 };

            var (data, totalRecords) = await repository.GetOrdersAsync(
                request,
                10,
                customerId: null
            );

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Status.Should().Be("Shipped");
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldSortByIdAscendingAndDescending()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "sortid@test.com");
            context.Customers.Add(customer);

            var o1 = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                ShippingAddress = "A",
                Subtotal = 10,
                TotalAmount = 10,
                IsDeleted = false,
            };
            var o2 = new Order
            {
                Id = 2,
                CustomerId = 1,
                Customer = customer,
                ShippingAddress = "B",
                Subtotal = 20,
                TotalAmount = 20,
                IsDeleted = false,
            };
            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var reqAsc = new GetOrdersRequestDto
            {
                SortBy = "id",
                SortOrder = "asc",
                Page = 1,
            };
            var (dataAsc, _) = await repository.GetOrdersAsync(reqAsc, 10, null);
            dataAsc.Should().HaveCount(2);
            dataAsc[0].Id.Should().Be(1);

            var reqDesc = new GetOrdersRequestDto
            {
                SortBy = "id",
                SortOrder = "desc",
                Page = 1,
            };
            var (dataDesc, _) = await repository.GetOrdersAsync(reqDesc, 10, null);
            dataDesc.Should().HaveCount(2);
            dataDesc[0].Id.Should().Be(2);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldSortByTotalAmountAscendingAndDescending()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "total@test.com");
            context.Customers.Add(customer);

            var o1 = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                ShippingAddress = "A",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };
            var o2 = new Order
            {
                Id = 2,
                CustomerId = 1,
                Customer = customer,
                ShippingAddress = "B",
                Subtotal = 100,
                TotalAmount = 100,
                IsDeleted = false,
            };
            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var reqAsc = new GetOrdersRequestDto
            {
                SortBy = "totalamount",
                SortOrder = "asc",
                Page = 1,
            };
            var (dataAsc, _) = await repository.GetOrdersAsync(reqAsc, 10, null);
            dataAsc.Should().HaveCount(2);
            dataAsc[0].TotalAmount.Should().Be(50);

            var reqDesc = new GetOrdersRequestDto
            {
                SortBy = "totalamount",
                SortOrder = "desc",
                Page = 1,
            };
            var (dataDesc, _) = await repository.GetOrdersAsync(reqDesc, 10, null);
            dataDesc.Should().HaveCount(2);
            dataDesc[0].TotalAmount.Should().Be(100);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldSortByStatusAscendingAndDescending()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "statusort@test.com");
            context.Customers.Add(customer);

            var o1 = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                Status = OrderStatus.Pending,
                ShippingAddress = "A",
                Subtotal = 10,
                TotalAmount = 10,
                IsDeleted = false,
            };
            var o2 = new Order
            {
                Id = 2,
                CustomerId = 1,
                Customer = customer,
                Status = OrderStatus.Delivered,
                ShippingAddress = "B",
                Subtotal = 10,
                TotalAmount = 10,
                IsDeleted = false,
            };
            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var reqAsc = new GetOrdersRequestDto
            {
                SortBy = "status",
                SortOrder = "asc",
                Page = 1,
            };
            var (dataAsc, _) = await repository.GetOrdersAsync(reqAsc, 10, null);
            dataAsc.Should().HaveCount(2);
            dataAsc[0].Status.Should().Be("Pending");

            var reqDesc = new GetOrdersRequestDto
            {
                SortBy = "status",
                SortOrder = "desc",
                Page = 1,
            };
            var (dataDesc, _) = await repository.GetOrdersAsync(reqDesc, 10, null);
            dataDesc.Should().HaveCount(2);
            dataDesc[0].Status.Should().Be("Delivered");
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldSortByDefaultOrderDateAscendingAndDescending()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "datesort@test.com");
            context.Customers.Add(customer);

            var o1 = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                OrderDate = new DateTime(2023, 1, 1),
                ShippingAddress = "A",
                Subtotal = 10,
                TotalAmount = 10,
                IsDeleted = false,
            };
            var o2 = new Order
            {
                Id = 2,
                CustomerId = 1,
                Customer = customer,
                OrderDate = new DateTime(2023, 1, 2),
                ShippingAddress = "B",
                Subtotal = 10,
                TotalAmount = 10,
                IsDeleted = false,
            };
            context.Orders.AddRange(o1, o2);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var reqAsc = new GetOrdersRequestDto
            {
                SortBy = "orderdate",
                SortOrder = "asc",
                Page = 1,
            };
            var (dataAsc, _) = await repository.GetOrdersAsync(reqAsc, 10, null);
            dataAsc.Should().HaveCount(2);
            dataAsc[0].Id.Should().Be(1);

            var reqDesc = new GetOrdersRequestDto
            {
                SortBy = "orderdate",
                SortOrder = "desc",
                Page = 1,
            };
            var (dataDesc, _) = await repository.GetOrdersAsync(reqDesc, 10, null);
            dataDesc.Should().HaveCount(2);
            dataDesc[0].Id.Should().Be(2);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldHandleNullCustomerAndNullPaymentProperly()
        {
            using var context = CreateDbContext();

            var customer = new Customer
            {
                Id = 1,
                Email = "",
                FirstName = "",
                LastName = "",
                PasswordHash = "hash",
                IsActive = true,
                IsDeleted = false,
            };
            context.Customers.Add(customer);

            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                Payment = null,
                ShippingAddress = "No Customer St",
                Subtotal = 50,
                TotalAmount = 50,
                IsDeleted = false,
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var request = new GetOrdersRequestDto { Page = 0 };

            var (data, totalRecords) = await repository.GetOrdersAsync(request, 10, null);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].CustomerName.Should().Be(string.Empty);
            data[0].CustomerEmail.Should().Be(string.Empty);
            data[0].Payment.Should().BeNull();
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldMapPaymentAndActiveOrderItemsWithProduct()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "orderpayment@test.com");
            context.Customers.Add(customer);

            var product = CreateProduct(1, "Chair", "SKU-CHAIR");
            context.Products.Add(product);

            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                ShippingAddress = "Address",
                Subtotal = 100,
                TotalAmount = 100,
                IsDeleted = false,
                Payment = new Payment
                {
                    Id = 1,
                    OrderId = 1,
                    Amount = 100,
                    PaymentMethod = PaymentMethod.Card,
                    PaymentStatus = PaymentStatus.Paid,
                    PaidAt = DateTime.UtcNow,
                },
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = 1,
                        Product = product,
                        Quantity = 2,
                        UnitPrice = 50,
                        LineTotal = 100,
                        IsDeleted = false,
                    },
                    new OrderItem
                    {
                        Id = 2,
                        OrderId = 1,
                        ProductId = 1,
                        Product = product,
                        Quantity = 1,
                        UnitPrice = 50,
                        LineTotal = 50,
                        IsDeleted = true,
                    },
                },
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var request = new GetOrdersRequestDto { Page = 1 };

            var (data, totalRecords) = await repository.GetOrdersAsync(request, 10, null);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Payment.Should().NotBeNull();
            data[0].Payment!.Amount.Should().Be(100);
            data[0].Items.Should().HaveCount(1);
            data[0].Items[0].ProductName.Should().Be("Chair");
            data[0].Items[0].Sku.Should().Be("SKU-CHAIR");
        }
    }
}
