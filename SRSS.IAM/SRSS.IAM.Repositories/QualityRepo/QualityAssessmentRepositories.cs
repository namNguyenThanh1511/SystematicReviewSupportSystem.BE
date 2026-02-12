using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.QualityRepo
{
	public interface IQualityAssessmentStrategyRepository : IGenericRepository<QualityAssessmentStrategy, Guid, AppDbContext>
	{
		Task<IEnumerable<QualityAssessmentStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default);
	}

	public interface IQualityChecklistRepository : IGenericRepository<QualityChecklist, Guid, AppDbContext>
	{
		Task<IEnumerable<QualityChecklist>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);
	}

	public interface IQualityCriterionRepository : IGenericRepository<QualityCriterion, Guid, AppDbContext>
	{
		Task<IEnumerable<QualityCriterion>> GetByChecklistIdAsync(Guid checklistId, CancellationToken cancellationToken = default);
	}

	public class QualityAssessmentStrategyRepository : GenericRepository<QualityAssessmentStrategy, Guid, AppDbContext>, IQualityAssessmentStrategyRepository
	{
		public QualityAssessmentStrategyRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<QualityAssessmentStrategy>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class QualityChecklistRepository : GenericRepository<QualityChecklist, Guid, AppDbContext>, IQualityChecklistRepository
	{
		public QualityChecklistRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<QualityChecklist>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(c => c.QaStrategyId == strategyId, isTracking: false, cancellationToken);
		}
	}

	public class QualityCriterionRepository : GenericRepository<QualityCriterion, Guid, AppDbContext>, IQualityCriterionRepository
	{
		public QualityCriterionRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<QualityCriterion>> GetByChecklistIdAsync(Guid checklistId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(c => c.ChecklistId == checklistId, isTracking: false, cancellationToken);
		}
	}
}