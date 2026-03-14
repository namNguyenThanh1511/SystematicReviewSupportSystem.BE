using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchStrategyRepo
{
	public class SearchSourceRepository : GenericRepository<SearchSource, Guid, AppDbContext>, ISearchSourceRepository
	{
		public SearchSourceRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<SearchSource>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}
}