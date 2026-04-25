using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.QualityRepo
{
    public interface IQualityAssessmentStrategyRepository : IGenericRepository<QualityAssessmentStrategy, Guid, AppDbContext>
    {
        Task<IEnumerable<QualityAssessmentStrategy>> GetByReviewProcessIdAsync(Guid reviewProcessId, CancellationToken cancellationToken = default);
        Task<IEnumerable<QualityAssessmentStrategy>> GetFullStrategyByReviewProcessIdAsync(Guid reviewProcessId, CancellationToken cancellationToken = default);
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

        public async Task<IEnumerable<QualityAssessmentStrategy>> GetByReviewProcessIdAsync(Guid reviewProcessId, CancellationToken cancellationToken = default)
        {
            return await FindAllAsync(s => s.ReviewProcessId == reviewProcessId, isTracking: false, cancellationToken);
        }

        public async Task<IEnumerable<QualityAssessmentStrategy>> GetFullStrategyByReviewProcessIdAsync(Guid reviewProcessId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentStrategy>()
                .Include(s => s.Checklists)
                .ThenInclude(c => c.Criteria)
                .AsNoTracking()
                .Where(s => s.ReviewProcessId == reviewProcessId)
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

    public class QualityAssessmentAssignmentRepository : GenericRepository<QualityAssessmentAssignment, Guid, AppDbContext>, IQualityAssessmentAssignmentRepository
    {
        public QualityAssessmentAssignmentRepository(AppDbContext context) : base(context) { }

        public async Task<QualityAssessmentAssignment?> GetWithPapersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.Papers)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<QualityAssessmentAssignment?> GetWithPapersByProcessAndUserAsync(Guid processId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.Papers)
                .FirstOrDefaultAsync(a => a.QualityAssessmentProcessId == processId && a.UserId == userId, cancellationToken);
        }

        public async Task<QualityAssessmentAssignment?> GetByUserAndQaPaperAsync(Guid userId, Guid qaPaperId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.Papers)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Papers.Any(p => p.Id == qaPaperId), cancellationToken);
        }

        public async Task<List<QualityAssessmentAssignment>> GetAllWithPapersByProcessIdAsync(Guid processId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentAssignment>()
                .Include(a => a.Papers)
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
                .Where(d => d.PaperId == qaPaperId)
                .ToListAsync(cancellationToken);
        }

        public async Task<QualityAssessmentDecision?> GetByQaPaperIdAndUserIdWithDetailsAsync(Guid qaPaperId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<QualityAssessmentDecision>()
                .Include(d => d.Reviewer)
                .Include(d => d.DecisionItems)
                .Where(d => d.PaperId == qaPaperId && d.ReviewerId == userId)
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