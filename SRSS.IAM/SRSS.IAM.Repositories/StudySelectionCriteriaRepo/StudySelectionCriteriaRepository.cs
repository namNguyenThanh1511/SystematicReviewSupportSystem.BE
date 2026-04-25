using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.StudySelectionCriteriaRepo;

namespace SRSS.IAM.Repositories.StudySelectionCriteriaRepo
{
	public class StudySelectionCriteriaRepository : GenericRepository<StudySelectionCriteria, Guid, AppDbContext>, IStudySelectionCriteriaRepository
	{
		public StudySelectionCriteriaRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<StudySelectionCriteria>> GetByStudySelectionProcessIdAsync(Guid studySelectionProcessId, CancellationToken cancellationToken = default)
		{
			return await GetQueryable(c => c.StudySelectionProcessId == studySelectionProcessId, isTracking: false)
				.Include(c => c.InclusionCriteria)
				.Include(c => c.ExclusionCriteria)
				.ToListAsync(cancellationToken);
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

	public class StudySelectionCriteriaAIResponseRepository : GenericRepository<StudySelectionCriteriaAIResponse, Guid, AppDbContext>, IStudySelectionCriteriaAIResponseRepository
	{
		public StudySelectionCriteriaAIResponseRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<StudySelectionCriteriaAIResponse>> GetByStudySelectionProcessIdAsync(Guid studySelectionProcessId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(c => c.StudySelectionProcessId == studySelectionProcessId, isTracking: false, cancellationToken);
		}
	}
}