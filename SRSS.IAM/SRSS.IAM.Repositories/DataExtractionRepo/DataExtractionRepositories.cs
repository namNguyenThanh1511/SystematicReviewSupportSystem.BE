using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
	public interface IDataExtractionStrategyRepository : IGenericRepository<DataExtractionStrategy, Guid, AppDbContext>
	{
		Task<IEnumerable<DataExtractionStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public interface IDataExtractionFormRepository : IGenericRepository<DataExtractionForm, Guid, AppDbContext>
	{
		Task<IEnumerable<DataExtractionForm>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);
	}

	public interface IDataItemDefinitionRepository : IGenericRepository<DataItemDefinition, Guid, AppDbContext>
	{
		Task<IEnumerable<DataItemDefinition>> GetByFormIdAsync(Guid formId, CancellationToken cancellationToken = default);
	}

	public class DataExtractionStrategyRepository : GenericRepository<DataExtractionStrategy, Guid, AppDbContext>, IDataExtractionStrategyRepository
	{
		public DataExtractionStrategyRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<DataExtractionStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class DataExtractionFormRepository : GenericRepository<DataExtractionForm, Guid, AppDbContext>, IDataExtractionFormRepository
	{
		public DataExtractionFormRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<DataExtractionForm>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(f => f.ExtractionStrategyId == strategyId, isTracking: false, cancellationToken);
		}
	}

	public class DataItemDefinitionRepository : GenericRepository<DataItemDefinition, Guid, AppDbContext>, IDataItemDefinitionRepository
	{
		public DataItemDefinitionRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<DataItemDefinition>> GetByFormIdAsync(Guid formId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(d => d.FormId == formId, isTracking: false, cancellationToken);
		}
	}
}