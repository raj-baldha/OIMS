using OIMS.Application.DTOs.Request;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;

namespace OIMS.Application.BackgroundJobs
{
    public class LowStockNotificationJob
    {
        private readonly IInventoryService _inventoryService;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;

        public LowStockNotificationJob(
            IInventoryService inventoryService,
            INotificationService notificationService,
            IUserRepository userRepository
        )
        {
            _inventoryService = inventoryService;
            _notificationService = notificationService;
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync()
        {
            // 1. Get low-stock products
            var lowStockProducts = await _inventoryService.GetLowStockProductsAsync();

            // Nothing to notify
            if (lowStockProducts.Count == 0)
            {
                return;
            }

            // 2. Get active Administrator and Manager users
            var users = await _userRepository.GetActiveUsersByRolesAsync(
                new List<string> { "Administrator", "Manager" }
            );

            // No users to notify
            if (users.Count == 0)
            {
                return;
            }

            // 3. Convert low-stock products into notification DTOs
            var products = lowStockProducts
                .Select(x => new LowStockNotificationItemDto
                {
                    ProductId = x.Id,
                    ProductName = x.Name,
                    Sku = x.Sku,
                    QuantityOnHand = x.QuantityOnHand,
                    MinStockLevel = x.MinStockLevel,
                })
                .ToList();

            // 4. Get user IDs
            var userIds = users.Select(x => x.Id).ToList();

            // 5. Create notifications
            await _notificationService.CreateLowStockNotificationsAsync(userIds, products);
        }
    }
}
