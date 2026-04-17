using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace SRSS.IAM.Repositories.StudySelectionChecklistRepo
{
    public interface IStudySelectionChecklistTemplateRepository : IGenericRepository<StudySelectionChecklistTemplate, Guid, AppDbContext>
    {
        Task<StudySelectionChecklistTemplate?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<StudySelectionChecklistTemplate>> GetAllByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<StudySelectionChecklistTemplate?> GetWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<StudySelectionChecklistTemplate?> GetByIdWithDetailsAsync(Guid templateId, Guid projectId, CancellationToken cancellationToken = default);
    }

    public interface IStudySelectionChecklistTemplateSectionRepository : IGenericRepository<StudySelectionChecklistTemplateSection, Guid, AppDbContext>
    {
    }

    public interface IStudySelectionChecklistTemplateItemRepository : IGenericRepository<StudySelectionChecklistTemplateItem, Guid, AppDbContext>
    {
    }

    public interface IStudySelectionChecklistSubmissionRepository : IGenericRepository<StudySelectionChecklistSubmission, Guid, AppDbContext>
    {
        Task<StudySelectionChecklistSubmission?> GetByDecisionIdAsync(Guid decisionId, CancellationToken cancellationToken = default);
        Task<StudySelectionChecklistSubmission?> GetByPaperAndPhaseAsync(Guid paperId, ScreeningPhase phase, CancellationToken cancellationToken = default);
    }


    public class StudySelectionChecklistTemplateRepository : GenericRepository<StudySelectionChecklistTemplate, Guid, AppDbContext>, IStudySelectionChecklistTemplateRepository
    {
        public StudySelectionChecklistTemplateRepository(AppDbContext context) : base(context) { }

        public async Task<StudySelectionChecklistTemplate?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionChecklistTemplates
                .Where(t => t.ProjectId == projectId)
                .OrderByDescending(t => t.IsActive)
                .ThenByDescending(t => t.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<StudySelectionChecklistTemplate>> GetAllByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionChecklistTemplates
                .Where(t => t.ProjectId == projectId)
                .OrderByDescending(t => t.Version)
                .ToListAsync(cancellationToken);
        }

        public async Task<StudySelectionChecklistTemplate?> GetWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionChecklistTemplates
                .Include(t => t.Sections.OrderBy(s => s.Order))
                    .ThenInclude(s => s.Items.OrderBy(i => i.Order))
                .Where(t => t.ProjectId == projectId)
                .OrderByDescending(t => t.IsActive)
                .ThenByDescending(t => t.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<StudySelectionChecklistTemplate?> GetByIdWithDetailsAsync(Guid templateId, Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionChecklistTemplates
                .Include(t => t.Sections.OrderBy(s => s.Order))
                    .ThenInclude(s => s.Items.OrderBy(i => i.Order))
                .FirstOrDefaultAsync(t => t.Id == templateId && t.ProjectId == projectId, cancellationToken);
        }
    }

    public class StudySelectionChecklistTemplateSectionRepository : GenericRepository<StudySelectionChecklistTemplateSection, Guid, AppDbContext>, IStudySelectionChecklistTemplateSectionRepository
    {
        public StudySelectionChecklistTemplateSectionRepository(AppDbContext context) : base(context) { }
    }

    public class StudySelectionChecklistTemplateItemRepository : GenericRepository<StudySelectionChecklistTemplateItem, Guid, AppDbContext>, IStudySelectionChecklistTemplateItemRepository
    {
        public StudySelectionChecklistTemplateItemRepository(AppDbContext context) : base(context) { }
    }

    public class StudySelectionChecklistSubmissionRepository : GenericRepository<StudySelectionChecklistSubmission, Guid, AppDbContext>, IStudySelectionChecklistSubmissionRepository
    {
        public StudySelectionChecklistSubmissionRepository(AppDbContext context) : base(context) { }

        public async Task<StudySelectionChecklistSubmission?> GetByDecisionIdAsync(Guid decisionId, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionChecklistSubmissions
                .FirstOrDefaultAsync(s => s.ScreeningDecisionId == decisionId, cancellationToken);
        }

        public async Task<StudySelectionChecklistSubmission?> GetByPaperAndPhaseAsync(Guid paperId, ScreeningPhase phase, CancellationToken cancellationToken = default)
        {
            // Join with ScreeningDecision to filter by PaperId and Phase
            return await _context.StudySelectionChecklistSubmissions
                .Include(s => s.ScreeningDecision)
                .Where(s => s.ScreeningDecision.PaperId == paperId && s.ScreeningDecision.Phase == phase)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
