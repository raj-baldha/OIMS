using OIMS.Application.DTOs.Orders;
using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Customer?> GetCustomerAsync(int customerId);

        Task<List<Product>> GetProductsAsync(
            List<int> productIds);

        Task AddOrderAsync(Order order);

        Task AddInventoryTransactionsAsync(
            List<InventoryTransaction> transactions);

        Task AddAuditLogAsync(AuditLog auditLog);

        Task SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

        Task<Order?> GetOrderForStatusUpdateAsync(
           int orderId);

        Task AddOrderStatusHistoryAsync(
            OrderStatusHistory statusHistory);

        Task<(List<OrderListResponseDto> Data, int TotalRecords)>
        GetOrdersAsync(
            GetOrdersRequestDto request,
            int pageSize,
            int? customerId);

        Task<Order?> GetOrderForInvoiceAsync(int orderId);
        Task AddOrderDocumentAsync(OrderDocument orderDocument);
    }
}