using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.NotificationRepo
{
    public interface INotificationRepository : IGenericRepository<Notification, Guid, AppDbContext>
    {
        Task<List<Notification>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<int> CountByUserIdAsync(Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
    }
}
