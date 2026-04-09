using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.MasterSearchSourceRepo
{
    public class MasterSearchSourceRepository : GenericRepository<MasterSearchSources, Guid, AppDbContext>, IMasterSearchSourceRepository
    {
        public MasterSearchSourceRepository(AppDbContext context) : base(context) { }

        public async Task<MasterSearchSources?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Set<MasterSearchSources>().FirstOrDefaultAsync(m => m.SourceName.ToLower() == name.ToLower(), cancellationToken);
        }

        public async Task<int> GetUsageCountAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.SearchSources.CountAsync(s => s.MasterSourceId == id, cancellationToken);
        }

        public async Task<Dictionary<Guid, int>> GetUsageCountsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _context.SearchSources
                .Where(s => s.MasterSourceId != null && ids.Contains(s.MasterSourceId.Value))
                .GroupBy(s => s.MasterSourceId)
                .Select(g => new { Id = g.Key.Value, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);
        }
    }
}
