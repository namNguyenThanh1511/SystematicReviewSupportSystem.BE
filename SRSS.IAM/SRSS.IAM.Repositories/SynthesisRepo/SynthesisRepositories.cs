using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SynthesisRepo
{
	public interface IDataSynthesisStrategyRepository : IGenericRepository<DataSynthesisStrategy, Guid, AppDbContext>
	{
		Task<IEnumerable<DataSynthesisStrategy>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	}

	public class DataSynthesisStrategyRepository : GenericRepository<DataSynthesisStrategy, Guid, AppDbContext>, IDataSynthesisStrategyRepository
	{
		public DataSynthesisStrategyRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<DataSynthesisStrategy>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProjectId == projectId, isTracking: false, cancellationToken);
		}
	}
}