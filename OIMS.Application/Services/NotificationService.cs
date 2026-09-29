using OIMS.Application.DTOs.Request;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Entities;

namespace OIMS.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task CreateLowStockNotificationsAsync(
            List<int> userIds,
            List<LowStockNotificationItemDto> products
        )
        {
            var notifications = new List<Notification>();

            foreach (var userId in userIds)
            {
                foreach (var product in products)
                {
                    notifications.Add(
                        new Notification
                        {
                            UserId = userId,

                            Title = "Low Stock Alert",

                            Message =
                                $"Product '{product.ProductName}' "
                                + $"(SKU: {product.Sku}) is low on stock. "
                                + $"Current stock: {product.QuantityOnHand}. "
                                + $"Minimum stock level: {product.MinStockLevel}.",

                            IsRead = false,

                            CreatedAt = DateTime.UtcNow,
                        }
                    );
                }
            }

            if (notifications.Count == 0)
            {
                return;
            }

            await _notificationRepository.AddRangeAsync(notifications);

            await _notificationRepository.SaveChangesAsync();
        }
    }
}
