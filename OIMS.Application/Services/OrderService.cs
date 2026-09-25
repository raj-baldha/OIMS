using Microsoft.Extensions.Configuration;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Orders;
using OIMS.Application.DTOs.Request;
using OIMS.Application.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IConfiguration _configuration; 

    public OrderService(IOrderRepository orderRepository, IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _configuration = configuration;
    }

    public async Task<CreatedOrderResponseDto> CreateOrderAsync(
        CreateOrderRequestDto request,
        int customerId)
    {
        var customer = await _orderRepository.GetCustomerAsync(customerId);
        if (customer is null)
        {
            throw new NotFoundException("Customer not found or inactive.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new BadRequestException("Order must contain at least one product.");
        }

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            throw new BadRequestException("Shipping address is required.");
        }

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var paymentMethod))
        {
            throw new BadRequestException("Invalid payment method.");
        }

        var productIds = request.Items
            .Select(x => x.ProductId)
            .ToList();

        if (productIds.Count != productIds.Distinct().Count())
        {
            throw new BadRequestException("The same product cannot be added multiple times.");
        }

        var products = await _orderRepository.GetProductsAsync(productIds);

        if (products.Count != productIds.Count)
        {
            throw new NotFoundException("One or more products were not found or are inactive.");
        }

        var productLookup = products.ToDictionary(p => p.Id);

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();
        var inventoryTransactions = new List<InventoryTransaction>();
        var now = DateTime.UtcNow;

        foreach (var requestItem in request.Items)
        {
            if (requestItem.Quantity <= 0)
            {
                throw new BadRequestException("Product quantity must be greater than zero.");
            }

            var product = productLookup[requestItem.ProductId];

            if (product.QuantityOnHand < requestItem.Quantity)
            {
                throw new BadRequestException($"Insufficient stock for product '{product.Name}'.");
            }

            int quantityBefore = product.QuantityOnHand;
            decimal lineTotal = product.SellingPrice * requestItem.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = requestItem.Quantity,
                UnitPrice = product.SellingPrice,
                LineTotal = lineTotal,
                CreatedAt = now,
                IsDeleted = false
            });

            product.QuantityOnHand -= requestItem.Quantity;
            product.UpdatedAt = now;
            product.UpdatedBy = customerId;
            product.Version++;

            inventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = product.Id,
                ChangeType = InventoryChangeType.OrderPlacement,
                QuantityBefore = quantityBefore,
                QuantityChange = -requestItem.Quantity,
                QuantityAfter = product.QuantityOnHand,
                Notes = "Stock decreased due to order placement.",
                CreatedBy = customerId,
                CreatedAt = now
            });
        }

        const decimal discountAmount = 0m;
        decimal totalAmount = subtotal - discountAmount;

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = now,
            Status = OrderStatus.Pending,
            ShippingAddress = request.ShippingAddress.Trim(),
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            CreatedAt = now,
            CreatedBy = customerId,
            IsDeleted = false,
            Payment = new Payment
            {
                Amount = totalAmount,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = now,
                CreatedBy = customerId
            }
        };

        foreach (var item in orderItems)
        {
            order.OrderItems.Add(item);
        }

        order.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Pending,
            ChangedBy = customerId,
            ChangedAt = now
        });

        string? invoiceUrl = null;

        try
        {
            await _orderRepository.BeginTransactionAsync();

            await _orderRepository.AddOrderAsync(order);
            await _orderRepository.AddInventoryTransactionsAsync(inventoryTransactions);
            await _orderRepository.SaveChangesAsync();

            var invoiceDto = new InvoicePdfDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                CustomerEmail = customer.Email,
                ShippingAddress = order.ShippingAddress,
                Subtotal = order.Subtotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.Payment.PaymentMethod.ToString(),
                PaymentStatus = order.Payment.PaymentStatus.ToString(),
                Items = order.OrderItems
                    .Select(x => new InvoiceItemDto
                    {
                        ProductName = productLookup[x.ProductId].Name,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        LineTotal = x.LineTotal
                    })
                    .ToList()
            };

            byte[] invoicePdf = InvoicePdfHelper.GenerateInvoicePdf(invoiceDto);

            invoiceUrl = await FileStorageHelper.SaveInvoiceAsync(order.Id, invoicePdf);

            var orderDocument = new OrderDocument
            {
                OrderId = order.Id,
                FileUrl = invoiceUrl,
                StoredFileName = $"invoice-{order.Id}.pdf",
                ContentType = "application/pdf",
                FileSize = invoicePdf.Length,
                CreatedAt = now,
                CreatedBy = customerId,
                IsDeleted = false
            };

            await _orderRepository.AddOrderDocumentAsync(orderDocument);

            var auditLog = new AuditLog
            {
                UserId = customerId,
                Action = "Created",
                EntityName = nameof(Order),
                EntityId = order.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepository.AddAuditLogAsync(auditLog);
            await _orderRepository.SaveChangesAsync();

            await _orderRepository.CommitTransactionAsync();
        }
        catch
        {
            await _orderRepository.RollbackTransactionAsync();

            if (!string.IsNullOrWhiteSpace(invoiceUrl))
            {
                try
                {
                    await FileStorageHelper.DeleteInvoiceAsync(invoiceUrl);
                }
                catch
                {
                }
            }

            throw;
        }

        return new CreatedOrderResponseDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            ShippingAddress = order.ShippingAddress,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.Payment.PaymentMethod.ToString(),
            PaymentStatus = order.Payment.PaymentStatus.ToString(),
            InvoiceUrl = invoiceUrl,
            Items = order.OrderItems
                .Select(x => new CreatedOrderItemResponseDto
                {
                    ProductId = x.ProductId,
                    ProductName = productLookup[x.ProductId].Name,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    LineTotal = x.LineTotal
                })
                .ToList()
        };
    }

    public async Task UpdateOrderStatusAsync(
    int orderId,
    UpdateOrderStatusRequestDto request,
    int userId)
    {
        var order = await _orderRepository.GetOrderForStatusUpdateAsync(orderId);
        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            throw new BadRequestException("Invalid order status.");
        }

        var currentStatus = order.Status;

        bool isValidTransition = (currentStatus, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Confirmed, OrderStatus.Processing) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            _ => false
        };

        if (!isValidTransition)
        {
            throw new BadRequestException(
                $"Invalid order status transition from {currentStatus} to {newStatus}.");
        }

        var now = DateTime.UtcNow;

        order.Status = newStatus;
        order.UpdatedAt = now;
        order.UpdatedBy = userId;

        var statusHistory = new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = newStatus,
            ChangedBy = userId,
            ChangedAt = now
        };

        await _orderRepository.AddOrderStatusHistoryAsync(statusHistory);
        await _orderRepository.SaveChangesAsync();
    }

    public async Task CancelOrderAsync(
    int orderId,
    int userId,
    string role)
    {
        var order = await _orderRepository.GetOrderForStatusUpdateAsync(orderId);
        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase) &&
            order.CustomerId != userId)
        {
            throw new ForbiddenException("You are not allowed to cancel this order.");
        }

        if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
        {
            throw new BadRequestException("Order cannot be cancelled at its current status.");
        }

        var now = DateTime.UtcNow;

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = now;
        order.UpdatedBy = userId;

        var statusHistory = new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Cancelled,
            ChangedBy = userId,
            ChangedAt = now
        };

        await _orderRepository.AddOrderStatusHistoryAsync(statusHistory);
        await _orderRepository.SaveChangesAsync();
    }

    public async Task<PagedResponseDto<OrderListResponseDto>> GetOrdersAsync(
    GetOrdersRequestDto request,
    int userId,
    string role)
    {
        int? customerId = string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase)
            ? userId
            : null;

        int pageSize = int.Parse(_configuration["Pagination:PageSize"]!);

        var result = await _orderRepository.GetOrdersAsync(
            request,
            pageSize,
            customerId);

        int page = request.Page < 1 ? 1 : request.Page;
        int totalPages = (int)Math.Ceiling((double)result.TotalRecords / pageSize);

        return new PagedResponseDto<OrderListResponseDto>
        {
            Data = result.Data,
            Page = page,
            PageSize = pageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = totalPages
        };
    }
}