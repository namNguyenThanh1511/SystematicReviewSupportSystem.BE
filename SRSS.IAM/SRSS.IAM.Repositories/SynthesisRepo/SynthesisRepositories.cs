using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SynthesisRepo
{
	public interface IDataSynthesisStrategyRepository : IGenericRepository<DataSynthesisStrategy, Guid, AppDbContext>
	{
		Task<IEnumerable<DataSynthesisStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public interface IDisseminationStrategyRepository : IGenericRepository<DisseminationStrategy, Guid, AppDbContext>
	{
		Task<IEnumerable<DisseminationStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public interface IProjectTimetableRepository : IGenericRepository<ProjectTimetable, Guid, AppDbContext>
	{
		Task<IEnumerable<ProjectTimetable>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public class DataSynthesisStrategyRepository : GenericRepository<DataSynthesisStrategy, Guid, AppDbContext>, IDataSynthesisStrategyRepository
	{
		public DataSynthesisStrategyRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<DataSynthesisStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class DisseminationStrategyRepository : GenericRepository<DisseminationStrategy, Guid, AppDbContext>, IDisseminationStrategyRepository
	{
		public DisseminationStrategyRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<DisseminationStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class ProjectTimetableRepository : GenericRepository<ProjectTimetable, Guid, AppDbContext>, IProjectTimetableRepository
	{
		public ProjectTimetableRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ProjectTimetable>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(t => t.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}
}