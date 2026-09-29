using Microsoft.EntityFrameworkCore;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task AddRangeAsync(List<Notification> notifications)
        {
            await _context.Notifications.AddRangeAsync(notifications);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
