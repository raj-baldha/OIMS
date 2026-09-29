using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task AddRangeAsync(List<Notification> notifications);
        Task SaveChangesAsync();
    }
}
