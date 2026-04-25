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
        Task<StudySelectionChecklistTemplate?> GetActiveWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default);
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
        Task<StudySelectionChecklistSubmission?> GetByContextWithAnswersAsync(Guid processId, Guid paperId, Guid reviewerId, ScreeningPhase phase, CancellationToken cancellationToken = default);
    }

    public interface IStudySelectionChecklistSubmissionSectionAnswerRepository : IGenericRepository<StudySelectionChecklistSubmissionSectionAnswer, Guid, AppDbContext>
    {
    }

    public interface IStudySelectionChecklistSubmissionItemAnswerRepository : IGenericRepository<StudySelectionChecklistSubmissionItemAnswer, Guid, AppDbContext>
    {
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

        public async Task<StudySelectionChecklistTemplate?> GetActiveWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionChecklistTemplates
                .Include(t => t.Sections.OrderBy(s => s.Order))
                    .ThenInclude(s => s.Items.OrderBy(i => i.Order))
                .Where(t => t.ProjectId == projectId && t.IsActive)
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

        public async Task<StudySelectionChecklistSubmission?> GetByContextWithAnswersAsync(Guid processId, Guid paperId, Guid reviewerId, ScreeningPhase phase, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionChecklistSubmissions
                .Include(s => s.SectionAnswers)
                .Include(s => s.ItemAnswers)
                .FirstOrDefaultAsync(s => s.StudySelectionProcessId == processId &&
                                         s.PaperId == paperId &&
                                         s.ReviewerId == reviewerId &&
                                         s.Phase == phase, cancellationToken);
        }
    }

    public class StudySelectionChecklistSubmissionSectionAnswerRepository : GenericRepository<StudySelectionChecklistSubmissionSectionAnswer, Guid, AppDbContext>, IStudySelectionChecklistSubmissionSectionAnswerRepository
    {
        public StudySelectionChecklistSubmissionSectionAnswerRepository(AppDbContext context) : base(context) { }
    }

    public class StudySelectionChecklistSubmissionItemAnswerRepository : GenericRepository<StudySelectionChecklistSubmissionItemAnswer, Guid, AppDbContext>, IStudySelectionChecklistSubmissionItemAnswerRepository
    {
        public StudySelectionChecklistSubmissionItemAnswerRepository(AppDbContext context) : base(context) { }
    }
}
