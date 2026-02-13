using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchStrategyRepo
{
	public class SearchStrategyRepository : GenericRepository<SearchStrategy, Guid, AppDbContext>, ISearchStrategyRepository
	{
		public SearchStrategyRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<SearchStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class SearchStringRepository : GenericRepository<SearchString, Guid, AppDbContext>, ISearchStringRepository
	{
		public SearchStringRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<SearchString>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.StrategyId == strategyId, isTracking: false, cancellationToken);
		}
	}

	public class SearchTermRepository : GenericRepository<SearchTerm, Guid, AppDbContext>, ISearchTermRepository
	{
		public SearchTermRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<SearchTerm>> GetBySearchStringIdAsync(Guid searchStringId, CancellationToken cancellationToken = default)
		{
			return await _context.SearchStringTerms
				.Where(st => st.SearchStringId == searchStringId)
				.Join(_context.SearchTerms,
					st => st.TermId,
					t => t.Id,
					(st, t) => t)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}
	}

	public class SearchStringTermRepository : GenericRepository<SearchStringTerm, Guid, AppDbContext>, ISearchStringTermRepository
	{
		public SearchStringTermRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<SearchStringTerm>> GetBySearchStringIdAsync(Guid searchStringId, CancellationToken cancellationToken = default)
		{
			return await _context.SearchStringTerms
				.AsNoTracking()
				.Where(st => st.SearchStringId == searchStringId)
				.ToListAsync(cancellationToken);
		}

		public async Task<bool> ExistsAsync(Guid searchStringId, Guid termId, CancellationToken cancellationToken = default)
		{
			return await _context.SearchStringTerms
				.AnyAsync(st => st.SearchStringId == searchStringId && st.TermId == termId, cancellationToken);
		}
	}

	public class SearchSourceRepository : GenericRepository<SearchSource, Guid, AppDbContext>, ISearchSourceRepository
	{
		public SearchSourceRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<SearchSource>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}
}