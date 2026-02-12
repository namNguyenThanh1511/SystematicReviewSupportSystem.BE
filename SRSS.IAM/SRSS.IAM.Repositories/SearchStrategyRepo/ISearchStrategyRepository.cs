using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchStrategyRepo
{
	public interface ISearchStrategyRepository : IGenericRepository<SearchStrategy, Guid, AppDbContext>
	{
		Task<IEnumerable<SearchStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public interface ISearchStringRepository : IGenericRepository<SearchString, Guid, AppDbContext>
	{
		Task<IEnumerable<SearchString>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);
	}

	public interface ISearchTermRepository : IGenericRepository<SearchTerm, Guid, AppDbContext>
	{
		Task<IEnumerable<SearchTerm>> GetBySearchStringIdAsync(Guid searchStringId, CancellationToken cancellationToken = default);
	}

	public interface ISearchStringTermRepository : IGenericRepository<SearchStringTerm, Guid, AppDbContext>
	{
		Task<IEnumerable<SearchStringTerm>> GetBySearchStringIdAsync(Guid searchStringId, CancellationToken cancellationToken = default);
		Task<bool> ExistsAsync(Guid searchStringId, Guid termId, CancellationToken cancellationToken = default);
	}

	public interface ISearchSourceRepository : IGenericRepository<SearchSource, Guid, AppDbContext>
	{
		Task<IEnumerable<SearchSource>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}
}