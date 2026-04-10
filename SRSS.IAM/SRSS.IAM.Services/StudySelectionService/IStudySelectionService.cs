using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.GrobidClient;

namespace SRSS.IAM.Services.StudySelectionService
{
    public interface IStudySelectionService
    {
        // Process Management
        Task<StudySelectionProcessResponse> CreateStudySelectionProcessAsync(
            CreateStudySelectionProcessRequest request,
            CancellationToken cancellationToken = default);

        Task<StudySelectionProcessResponse> GetStudySelectionProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<StudySelectionProcessResponse> StartStudySelectionProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<StudySelectionProcessResponse> CompleteStudySelectionProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        // Paper Management
        Task<List<Guid>> GetEligiblePapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<List<Guid>> GetFullTextEligiblePapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        // Decision Management
        Task<ScreeningDecisionResponse> SubmitScreeningDecisionAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            SubmitScreeningDecisionRequest request,
            CancellationToken cancellationToken = default);



        Task<List<ScreeningDecisionResponse>> GetDecisionsByPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default);

        // Conflict Management
        Task<List<ConflictedPaperResponse>> GetConflictedPapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PhaseConflictedPaperResponse>> GetConflictedPapersByPhaseAsync(
            Guid studySelectionProcessId,
            ConflictedPapersRequest request,
            CancellationToken cancellationToken = default);

        Task<ScreeningResolutionResponse> ResolveConflictAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ResolveScreeningConflictRequest request,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<ScreeningResolutionPaperResponse>> GetResolutionsAsync(
            Guid studySelectionProcessId,
            GetResolutionsRequest request,
            CancellationToken cancellationToken = default);

        Task<ConflictPaperDetailResponse> GetConflictPaperDetailAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default);

        // Status and Statistics
        Task<PaperSelectionStatus> GetPaperSelectionStatusAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default);

        Task<SelectionStatisticsResponse> GetSelectionStatisticsAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<SelectionStatisticsResponse> GetPhaseStatisticsAsync(
            Guid studySelectionProcessId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default);

        Task<StudySelectionPhaseStatusResponse> GetPhaseStatusAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperWithDecisionsResponse>> GetPapersWithDecisionsAsync(
            Guid studySelectionProcessId,
            PapersWithDecisionsRequest request,
            CancellationToken cancellationToken = default);

        Task<PaperWithDecisionsResponse> GetPaperDetailsAsync(Guid studySelectionProcessId, Guid paperId,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperWithDecisionsResponse>> GetAssignedPapersAsync(
            Guid studySelectionProcessId,
            Guid userId,
            PapersWithDecisionsRequest request,
            CancellationToken cancellationToken = default);

        // Title-Abstract Screening Lifecycle
        Task<TitleAbstractScreeningResponse> CreateTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<TitleAbstractScreeningResponse> StartTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<TitleAbstractScreeningResponse> CompleteTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<TitleAbstractScreeningResponse> GetTitleAbstractScreeningAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        // Issue 2: Full-text upload/link management
        Task<PaperWithDecisionsResponse> UpdatePaperFullTextAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            UpdatePaperFullTextRequest request,
            CancellationToken cancellationToken = default);

        Task<PaperWithDecisionsResponse> RetryMetadataExtractionAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            RetryExtractionRequest request,
            CancellationToken cancellationToken = default);

        // Background tasks
        Task ProcessGrobidExtractionAsync(GrobidWorkItem workItem, CancellationToken ct);
    }
}
