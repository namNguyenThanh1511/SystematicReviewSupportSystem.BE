using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.UserTagInventoryRepo
{
    public class UserTagInventoryRepository : GenericRepository<UserTagInventory, Guid, AppDbContext>, IUserTagInventoryRepository
    {
        public UserTagInventoryRepository(AppDbContext context) : base(context) { }

        public async Task<List<UserTagInventory>> GetByUserAndPhaseAsync(Guid userId, ProcessPhase phase, CancellationToken cancellationToken = default)
        {
            return await _context.UserTagInventories
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Phase == phase)
                .OrderByDescending(t => t.UsageCount)
                .ThenBy(t => t.Label)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserTagInventory>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserTagInventories
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Phase)
                .ThenByDescending(t => t.UsageCount)
                .ThenBy(t => t.Label)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserTagInventory?> GetExistingEntryAsync(Guid userId, ProcessPhase phase, string label, CancellationToken cancellationToken = default)
        {
            return await _context.UserTagInventories
                .Where(t => t.UserId == userId && t.Phase == phase && t.Label == label)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
