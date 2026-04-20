using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.QualityRepo
{
	public interface IQualityAssessmentStrategyRepository : IGenericRepository<QualityAssessmentStrategy, Guid, AppDbContext>
	{
		Task<IEnumerable<QualityAssessmentStrategy>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<QualityAssessmentStrategy>> GetFullStrategyByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	}

	public interface IQualityChecklistRepository : IGenericRepository<QualityChecklist, Guid, AppDbContext>
	{
		Task<IEnumerable<QualityChecklist>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);
	}

	public interface IQualityCriterionRepository : IGenericRepository<QualityCriterion, Guid, AppDbContext>
	{
		Task<IEnumerable<QualityCriterion>> GetByChecklistIdAsync(Guid checklistId, CancellationToken cancellationToken = default);
	}

    public interface IQualityAssessmentProcessRepository : IGenericRepository<QualityAssessmentProcess, Guid, AppDbContext>
    {
    }

    public interface IQualityAssessmentPaperRepository : IGenericRepository<QualityAssessmentPaper, Guid, AppDbContext>
    {
        public Task<QualityAssessmentPaper?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        public Task<List<QualityAssessmentPaper>> GetByProcessIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IQualityAssessmentAssignmentRepository : IGenericRepository<QualityAssessmentAssignment, Guid, AppDbContext>
    {
        Task<QualityAssessmentAssignment?> GetWithPapersAsync(Guid id, CancellationToken cancellationToken = default);
        Task<QualityAssessmentAssignment?> GetWithPapersByProcessAndUserAsync(Guid processId, Guid userId, CancellationToken cancellationToken = default);
        Task<QualityAssessmentAssignment?> GetByUserAndQaPaperAsync(Guid userId, Guid qaPaperId, CancellationToken cancellationToken = default);
        Task<List<QualityAssessmentAssignment>> GetAllWithPapersByProcessIdAsync(Guid processId, CancellationToken cancellationToken = default);
    }

    public interface IQualityAssessmentDecisionRepository : IGenericRepository<QualityAssessmentDecision, Guid, AppDbContext>
    {
        Task<List<QualityAssessmentDecision>> GetByQaPaperIdWithDetailsAsync(Guid qaPaperId, CancellationToken cancellationToken = default);
        Task<QualityAssessmentDecision?> GetByQaPaperIdAndUserIdWithDetailsAsync(Guid qaPaperId, Guid userId, CancellationToken cancellationToken = default);
        Task<QualityAssessmentDecision?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public interface IQualityAssessmentResolutionRepository : IGenericRepository<QualityAssessmentResolution, Guid, AppDbContext>
    {
    }

	public class QualityAssessmentStrategyRepository : GenericRepository<QualityAssessmentStrategy, Guid, AppDbContext>, IQualityAssessmentStrategyRepository
	{
		public QualityAssessmentStrategyRepository(AppDbContext context) : base(context) { }

		public async Task<IEnumerable<QualityAssessmentStrategy>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await FindAllAsync(s => s.ProjectId == projectId, isTracking: false, cancellationToken);
		}

        public async Task<IEnumerable<QualityAssessmentStrategy>> GetFullStrategyByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentStrategy>()
                .Include(s => s.Checklists)
                .ThenInclude(c => c.Criteria)
                .AsNoTracking()
                .Where(s => s.ProjectId == projectId)
                .ToListAsync(cancellationToken);
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

    public class QualityAssessmentProcessRepository : GenericRepository<QualityAssessmentProcess, Guid, AppDbContext>, IQualityAssessmentProcessRepository
    {
        public QualityAssessmentProcessRepository(AppDbContext context) : base(context) { }
    }

    public class QualityAssessmentPaperRepository : GenericRepository<QualityAssessmentPaper, Guid, AppDbContext>, IQualityAssessmentPaperRepository
    {
        public QualityAssessmentPaperRepository(AppDbContext context) : base(context) { }

        public async Task<QualityAssessmentPaper?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentPaper>()
                .Include(p => p.Paper)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<List<QualityAssessmentPaper>> GetByProcessIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentPaper>()
                .Include(p => p.Paper)
                .Where(p => p.QualityAssessmentProcessId == id)
                .ToListAsync(cancellationToken);
        }
    }

    public class QualityAssessmentAssignmentRepository : GenericRepository<QualityAssessmentAssignment, Guid, AppDbContext>, IQualityAssessmentAssignmentRepository
    {
        public QualityAssessmentAssignmentRepository(AppDbContext context) : base(context) { }

        public async Task<QualityAssessmentAssignment?> GetWithPapersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.QualityAssessmentPapers)
                .ThenInclude(p => p.Paper)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<QualityAssessmentAssignment?> GetWithPapersByProcessAndUserAsync(Guid processId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.QualityAssessmentPapers)
                .ThenInclude(p => p.Paper)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.QualityAssessmentProcessId == processId && a.UserId == userId, cancellationToken);
        }

        public async Task<QualityAssessmentAssignment?> GetByUserAndQaPaperAsync(Guid userId, Guid qaPaperId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.QualityAssessmentPapers)
                .ThenInclude(p => p.Paper)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.QualityAssessmentPapers.Any(p => p.Id == qaPaperId), cancellationToken);
        }

        public async Task<List<QualityAssessmentAssignment>> GetAllWithPapersByProcessIdAsync(Guid processId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.QualityAssessmentPapers)
                .ThenInclude(p => p.Paper)
                .Include(a => a.User)
                .Where(a => a.QualityAssessmentProcessId == processId)
                .ToListAsync(cancellationToken);
        }
    }

    public class QualityAssessmentDecisionRepository : GenericRepository<QualityAssessmentDecision, Guid, AppDbContext>, IQualityAssessmentDecisionRepository
    {
        public QualityAssessmentDecisionRepository(AppDbContext context) : base(context) { }

        public async Task<List<QualityAssessmentDecision>> GetByQaPaperIdWithDetailsAsync(Guid qaPaperId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentDecision>()
                .Include(d => d.Reviewer)
                .Include(d => d.DecisionItems)
                .Where(d => d.QualityAssessmentPaperId == qaPaperId)
                .ToListAsync(cancellationToken);
        }

        public async Task<QualityAssessmentDecision?> GetByQaPaperIdAndUserIdWithDetailsAsync(Guid qaPaperId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentDecision>()
                .Include(d => d.Reviewer)
                .Include(d => d.DecisionItems)
                .Where(d => d.QualityAssessmentPaperId == qaPaperId && d.ReviewerId == userId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<QualityAssessmentDecision?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentDecision>()
                .Include(d => d.Reviewer)
                .Include(d => d.DecisionItems)
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }

    public class QualityAssessmentResolutionRepository : GenericRepository<QualityAssessmentResolution, Guid, AppDbContext>, IQualityAssessmentResolutionRepository
    {
        public QualityAssessmentResolutionRepository(AppDbContext context) : base(context) { }
    }
}