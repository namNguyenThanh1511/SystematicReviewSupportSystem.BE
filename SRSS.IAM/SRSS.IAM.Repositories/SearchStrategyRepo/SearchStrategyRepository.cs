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
			return await FindAllAsync(s => s.ProjectId == projectId, isTracking: false, cancellationToken);
		}
	}
}