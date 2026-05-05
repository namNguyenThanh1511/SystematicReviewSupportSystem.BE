using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchStrategyRepo
{
	public class SearchSourceRepository : GenericRepository<SearchSource, Guid, AppDbContext>, ISearchSourceRepository
	{
		public SearchSourceRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<SearchSource>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await _context.SearchSources
				.Include(s => s.MasterSource)
				.Include(s => s.Strategies)
				.Where(s => s.ProjectId == projectId)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<SearchSource?> GetByIdWithStrategiesAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await _context.SearchSources
				.Include(s => s.Strategies)
				.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
		}
	}
}