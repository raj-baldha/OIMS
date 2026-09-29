using OIMS.Application.DTOs.Request;

namespace OIMS.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task CreateLowStockNotificationsAsync(
            List<int> userIds,
            List<LowStockNotificationItemDto> products
        );
    }
}
