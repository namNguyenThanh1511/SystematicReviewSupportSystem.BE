using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.NotificationRepo
{
    public class NotificationRepository : GenericRepository<Notification, Guid, AppDbContext>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId);
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                }
                // No need to call SaveChanges here, UnitOfWork will handle it or user can call it manually.
                // However, the requirement said "MarkAllAsReadAsync(Guid userId)" in repository.
                // In Clean Architecture, repositories usually shouldn't call SaveChanges unless it's a specific pattern.
                // But looking at GenericRepository, it might not have SaveChanges.
            }
        }
    }
}
