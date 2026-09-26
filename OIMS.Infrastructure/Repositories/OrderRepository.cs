using Microsoft.EntityFrameworkCore;
using OIMS.Application.DTOs.Orders;
using OIMS.Application.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;

namespace OIMS.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetCustomerAsync(int customerId)
        {
            return await _context
                .Customers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == customerId && !x.IsDeleted && x.IsActive);
        }

        public async Task<List<Product>> GetProductsAsync(List<int> productIds)
        {
            return await _context
                .Products.Where(x => productIds.Contains(x.Id) && !x.IsDeleted && x.IsActive)
                .ToListAsync();
        }

        public async Task AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task AddInventoryTransactionsAsync(List<InventoryTransaction> transactions)
        {
            await _context.InventoryTransactions.AddRangeAsync(transactions);
        }

        public async Task AddAuditLogAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            var transaction = _context.Database.CurrentTransaction;

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            var transaction = _context.Database.CurrentTransaction;

            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
        }

        public async Task<Order?> GetOrderForStatusUpdateAsync(int orderId)
        {
            return await _context
                .Orders.Include(x => x.OrderItems)
                .Include(x => x.Payment)
                .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDeleted);
        }

        public async Task AddOrderStatusHistoryAsync(OrderStatusHistory statusHistory)
        {
            await _context.OrderStatusHistories.AddAsync(statusHistory);
        }

        public async Task<(List<OrderListResponseDto> Data, int TotalRecords)> GetOrdersAsync(
            GetOrdersRequestDto request,
            int pageSize,
            int? customerId
        )
        {
            var query = _context.Orders.AsNoTracking().Where(x => !x.IsDeleted);

            if (customerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim();

                if (int.TryParse(search, out int orderId))
                {
                    query = query.Where(x =>
                        x.Id == orderId
                        || x.Customer!.FirstName.Contains(search)
                        || x.Customer.LastName.Contains(search)
                        || x.Customer.Email.Contains(search)
                    );
                }
                else
                {
                    query = query.Where(x =>
                        x.Customer!.FirstName.Contains(search)
                        || x.Customer.LastName.Contains(search)
                        || x.Customer.Email.Contains(search)
                    );
                }
            }

            if (
                !string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status)
            )
            {
                query = query.Where(x => x.Status == status);
            }

            int totalRecords = await query.CountAsync();

            string sortBy = request.SortBy?.ToLowerInvariant() ?? "orderdate";
            bool isAscending = string.Equals(
                request.SortOrder,
                "asc",
                StringComparison.OrdinalIgnoreCase
            );

            query = (sortBy, isAscending) switch
            {
                ("id", true) => query.OrderBy(x => x.Id),
                ("id", false) => query.OrderByDescending(x => x.Id),
                ("totalamount", true) => query.OrderBy(x => x.TotalAmount),
                ("totalamount", false) => query.OrderByDescending(x => x.TotalAmount),
                ("status", true) => query.OrderBy(x => x.Status),
                ("status", false) => query.OrderByDescending(x => x.Status),
                (_, true) => query.OrderBy(x => x.OrderDate),
                _ => query.OrderByDescending(x => x.OrderDate),
            };

            int page = request.Page < 1 ? 1 : request.Page;
            int skip = (page - 1) * pageSize;

            var data = await query
                .Skip(skip)
                .Take(pageSize)
                //.Paginate(skip, pageSize)
                .Select(x => new OrderListResponseDto
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    CustomerName =
                        x.Customer != null
                            ? $"{x.Customer.FirstName} {x.Customer.LastName}".Trim()
                            : string.Empty,
                    CustomerEmail = x.Customer != null ? x.Customer.Email : string.Empty,
                    OrderDate = x.OrderDate,
                    Status = x.Status.ToString(),
                    Subtotal = x.Subtotal,
                    DiscountAmount = x.DiscountAmount,
                    TotalAmount = x.TotalAmount,
                    ShippingAddress = x.ShippingAddress,
                    Payment =
                        x.Payment == null
                            ? null
                            : new PaymentOrderResponseDto
                            {
                                Id = x.Payment.Id,
                                Amount = x.Payment.Amount,
                                PaymentMethod = x.Payment.PaymentMethod.ToString(),
                                PaymentStatus = x.Payment.PaymentStatus.ToString(),
                                PaidAt = x.Payment.PaidAt,
                            },
                    Items = x
                        .OrderItems.Where(item => !item.IsDeleted)
                        .Select(item => new OrderItemResponseDto
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            ProductName = item.Product != null ? item.Product.Name : string.Empty,
                            Sku = item.Product != null ? item.Product.Sku : string.Empty,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            LineTotal = item.LineTotal,
                        })
                        .ToList(),
                })
                .ToListAsync();

            return (data, totalRecords);
        }

        public async Task<Order?> GetOrderForInvoiceAsync(int orderId)
        {
            return await _context
                .Orders.AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Payment)
                .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDeleted);
        }

        public async Task AddOrderDocumentAsync(OrderDocument orderDocument)
        {
            await _context.OrderDocuments.AddAsync(orderDocument);
        }
    }
}
