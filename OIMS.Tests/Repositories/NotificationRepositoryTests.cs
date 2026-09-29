using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Infrastructure.Repositories;
using Xunit;

namespace OIMS.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNotificationToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new NotificationRepository(context);

            var notification = new Notification
            {
                UserId = 1,
                Title = "Order Placed",
                Message = "Your order #101 has been placed successfully.",
                IsRead = false,
            };

            await repository.AddAsync(notification);
            await repository.SaveChangesAsync();

            var savedNotification = await context.Notifications.FirstOrDefaultAsync(n =>
                n.UserId == 1
            );

            savedNotification.Should().NotBeNull();
            savedNotification!.Title.Should().Be("Order Placed");
            savedNotification.Message.Should().Be("Your order #101 has been placed successfully.");
            savedNotification.IsRead.Should().BeFalse();
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddMultipleNotificationsToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new NotificationRepository(context);

            var notifications = new List<Notification>
            {
                new Notification
                {
                    UserId = 1,
                    Title = "Notice 1",
                    Message = "Message 1",
                },
                new Notification
                {
                    UserId = 2,
                    Title = "Notice 2",
                    Message = "Message 2",
                },
            };

            await repository.AddRangeAsync(notifications);
            await repository.SaveChangesAsync();

            var savedNotifications = await context.Notifications.ToListAsync();

            savedNotifications.Should().HaveCount(2);
            savedNotifications.Should().Contain(n => n.Title == "Notice 1" && n.UserId == 1);
            savedNotifications.Should().Contain(n => n.Title == "Notice 2" && n.UserId == 2);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistStateChanges()
        {
            using var context = CreateDbContext();
            var repository = new NotificationRepository(context);

            var notification = new Notification
            {
                UserId = 5,
                Title = "Alert",
                Message = "System update",
                IsRead = false,
            };

            await repository.AddAsync(notification);
            await repository.SaveChangesAsync();

            notification.IsRead = true;
            await repository.SaveChangesAsync();

            var updatedNotification = await context.Notifications.FindAsync(notification.Id);
            updatedNotification.Should().NotBeNull();
            updatedNotification!.IsRead.Should().BeTrue();
        }
    }
}
