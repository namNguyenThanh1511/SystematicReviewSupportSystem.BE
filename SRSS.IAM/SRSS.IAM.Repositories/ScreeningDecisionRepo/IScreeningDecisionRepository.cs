using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.ScreeningDecisionRepo
{
    public interface IScreeningDecisionRepository : IGenericRepository<ScreeningDecision, Guid, AppDbContext>
    {
        Task<List<ScreeningDecision>> GetByProcessAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<List<ScreeningDecision>> GetByPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default);

        Task<ScreeningDecision?> GetByReviewerAndPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default);

        Task<bool> HasDecisionAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default);

        Task<List<Guid>> GetPapersWithConflictsAsync(
            Guid studySelectionProcessId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default);
        Task<int> CountScreenedPapersAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

    }
}
