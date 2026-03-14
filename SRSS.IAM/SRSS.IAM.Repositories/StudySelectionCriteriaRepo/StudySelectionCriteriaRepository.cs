using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.StudySelectionCriteriaRepo;

namespace SRSS.IAM.Repositories.StudySelectionCriteriaRepo
{
	public class StudySelectionCriteriaRepository : GenericRepository<StudySelectionCriteria, Guid, AppDbContext>, IStudySelectionCriteriaRepository
	{
		public StudySelectionCriteriaRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<StudySelectionCriteria>> GetByProtocolIdAsync(Guid protocolId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(c => c.ProtocolId == protocolId, isTracking: false, cancellationToken);
		}
	}

	public class InclusionCriterionRepository : GenericRepository<InclusionCriterion, Guid, AppDbContext>, IInclusionCriterionRepository
	{
		public InclusionCriterionRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<InclusionCriterion>> GetByCriteriaIdAsync(Guid criteriaId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(c => c.CriteriaId == criteriaId, isTracking: false, cancellationToken);
		}
	}

	public class ExclusionCriterionRepository : GenericRepository<ExclusionCriterion, Guid, AppDbContext>, IExclusionCriterionRepository
	{
		public ExclusionCriterionRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<ExclusionCriterion>> GetByCriteriaIdAsync(Guid criteriaId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(c => c.CriteriaId == criteriaId, isTracking: false, cancellationToken);
		}
	}
}