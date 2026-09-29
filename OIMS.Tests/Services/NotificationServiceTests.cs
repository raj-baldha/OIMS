using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OIMS.Application.DTOs.Request;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using Xunit;

namespace OIMS.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _notificationService = new NotificationService(_notificationRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateLowStockNotificationsAsync_ShouldReturnEarly_WhenUserIdsListIsEmpty()
        {
            var userIds = new List<int>();
            var products = new List<LowStockNotificationItemDto>
            {
                new LowStockNotificationItemDto
                {
                    ProductName = "Mechanical Keyboard",
                    Sku = "SKU-KEY-01",
                    QuantityOnHand = 2,
                    MinStockLevel = 10,
                },
            };

            await _notificationService.CreateLowStockNotificationsAsync(userIds, products);

            _notificationRepositoryMock.Verify(
                r => r.AddRangeAsync(It.IsAny<List<Notification>>()),
                Times.Never
            );
            _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateLowStockNotificationsAsync_ShouldReturnEarly_WhenProductsListIsEmpty()
        {
            var userIds = new List<int> { 1, 2 };
            var products = new List<LowStockNotificationItemDto>();

            await _notificationService.CreateLowStockNotificationsAsync(userIds, products);

            _notificationRepositoryMock.Verify(
                r => r.AddRangeAsync(It.IsAny<List<Notification>>()),
                Times.Never
            );
            _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateLowStockNotificationsAsync_ShouldSaveNotifications_WhenUsersAndProductsExist()
        {
            var userIds = new List<int> { 10, 20 };
            var products = new List<LowStockNotificationItemDto>
            {
                new LowStockNotificationItemDto
                {
                    ProductName = "Wireless Mouse",
                    Sku = "SKU-MOU-01",
                    QuantityOnHand = 1,
                    MinStockLevel = 5,
                },
                new LowStockNotificationItemDto
                {
                    ProductName = "USB-C Hub",
                    Sku = "SKU-HUB-02",
                    QuantityOnHand = 3,
                    MinStockLevel = 8,
                },
            };

            List<Notification>? capturedNotifications = null;

            _notificationRepositoryMock
                .Setup(r => r.AddRangeAsync(It.IsAny<List<Notification>>()))
                .Callback<List<Notification>>(list => capturedNotifications = list)
                .Returns(Task.CompletedTask);

            _notificationRepositoryMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _notificationService.CreateLowStockNotificationsAsync(userIds, products);

            capturedNotifications.Should().NotBeNull();
            capturedNotifications.Should().HaveCount(4);

            capturedNotifications
                .Should()
                .Contain(n =>
                    n.UserId == 10
                    && n.Title == "Low Stock Alert"
                    && n.Message.Contains("Wireless Mouse")
                    && n.Message.Contains("SKU-MOU-01")
                    && n.Message.Contains("Current stock: 1")
                    && n.Message.Contains("Minimum stock level: 5")
                    && !n.IsRead
                );

            capturedNotifications
                .Should()
                .Contain(n =>
                    n.UserId == 20
                    && n.Title == "Low Stock Alert"
                    && n.Message.Contains("USB-C Hub")
                    && n.Message.Contains("SKU-HUB-02")
                    && n.Message.Contains("Current stock: 3")
                    && n.Message.Contains("Minimum stock level: 8")
                    && !n.IsRead
                );

            _notificationRepositoryMock.Verify(
                r => r.AddRangeAsync(It.IsAny<List<Notification>>()),
                Times.Once
            );
            _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
