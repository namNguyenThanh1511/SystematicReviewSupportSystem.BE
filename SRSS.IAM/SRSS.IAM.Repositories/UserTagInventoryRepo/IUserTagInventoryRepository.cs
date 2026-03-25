using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.UserTagInventoryRepo
{
    public interface IUserTagInventoryRepository : IGenericRepository<UserTagInventory, Guid, AppDbContext>
    {
        Task<List<UserTagInventory>> GetByUserAndPhaseAsync(Guid userId, ProcessPhase phase, CancellationToken cancellationToken = default);

        Task<List<UserTagInventory>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<UserTagInventory?> GetExistingEntryAsync(Guid userId, ProcessPhase phase, string label, CancellationToken cancellationToken = default);
    }
}
