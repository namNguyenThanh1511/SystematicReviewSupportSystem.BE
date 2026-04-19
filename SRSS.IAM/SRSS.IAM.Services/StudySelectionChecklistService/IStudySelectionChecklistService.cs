using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.StudySelectionChecklist;

namespace SRSS.IAM.Services.StudySelectionChecklists
{
    public interface IStudySelectionChecklistService
    {
        // Template
        Task<StudySelectionChecklistTemplateDto> CreateTemplateAsync(Guid projectId, CreateStudySelectionChecklistTemplateRequest request, CancellationToken cancellationToken = default);

        Task<IEnumerable<StudySelectionChecklistTemplateSummaryDto>> GetTemplatesByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<StudySelectionChecklistTemplateDto> GetTemplateDetailAsync(Guid projectId, Guid templateId, CancellationToken cancellationToken = default);
        Task<bool> ActivateTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);

        // Reviewer specialized
        Task<PaperChecklistResponse> GetChecklistForPaperAsync(Guid processId, Guid paperId, ScreeningPhase phase, CancellationToken cancellationToken = default);

        // Document Live Review Export
        Task<LiveReviewChecklistDto> GetLiveReviewChecklistByProcessAsync(Guid studySelectionProcessId, CancellationToken cancellationToken = default);
    }

    public interface IStudySelectionChecklistSubmissionService
    {
        Task<ChecklistSubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request, CancellationToken cancellationToken = default);
        Task<ChecklistSubmissionDto> GetSubmissionAsync(Guid submissionId, CancellationToken cancellationToken = default);
        Task<ChecklistReviewDto> GetChecklistForReviewByContextAsync(Guid processId, Guid paperId, Guid reviewerId, ScreeningPhase phase, CancellationToken cancellationToken = default);
        Task<ChecklistReviewDto> GetSubmissionByContextAsync(Guid processId, Guid paperId, Guid reviewerId, ScreeningPhase phase, CancellationToken cancellationToken = default);
    }
}
