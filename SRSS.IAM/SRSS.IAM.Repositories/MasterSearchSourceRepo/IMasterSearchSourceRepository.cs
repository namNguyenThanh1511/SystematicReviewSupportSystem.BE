using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.MasterSearchSourceRepo
{
    public interface IMasterSearchSourceRepository : IGenericRepository<MasterSearchSources, Guid, AppDbContext>
    {
        Task<MasterSearchSources?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> GetUsageCountsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    }
}
