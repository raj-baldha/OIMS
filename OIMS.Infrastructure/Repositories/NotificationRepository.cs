using Microsoft.EntityFrameworkCore;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        /// <summary>Initializes the repository with the application database context.</summary>
        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Adds a notification to the database context.</summary>
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        /// <summary>Adds multiple notifications to the database context.</summary>
        public async Task AddRangeAsync(List<Notification> notifications)
        {
            await _context.Notifications.AddRangeAsync(notifications);
        }

        /// <summary>Persists pending database changes.</summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
