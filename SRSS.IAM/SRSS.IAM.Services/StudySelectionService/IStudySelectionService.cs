using SRSS.IAM.Services.DTOs.StudySelection;

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

        Task<ScreeningResolutionResponse> ResolveConflictAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ResolveScreeningConflictRequest request,
            CancellationToken cancellationToken = default);

        // Status and Statistics
        Task<PaperSelectionStatus> GetPaperSelectionStatusAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default);

        Task<SelectionStatisticsResponse> GetSelectionStatisticsAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<List<PaperWithDecisionsResponse>> GetPapersWithDecisionsAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);
    }
}
