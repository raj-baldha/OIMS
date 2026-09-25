using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Orders;
using OIMS.Application.DTOs.Request;

namespace OIMS.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<CreatedOrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request,int customerId);
        Task UpdateOrderStatusAsync(int orderId,UpdateOrderStatusRequestDto request,int userId);
        Task CancelOrderAsync(int orderId,int userId,string role);
        Task<PagedResponseDto<OrderListResponseDto>> GetOrdersAsync(GetOrdersRequestDto request,int userId,string role);
    }
}