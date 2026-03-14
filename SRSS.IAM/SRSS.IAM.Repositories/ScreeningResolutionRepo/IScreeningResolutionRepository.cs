using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.ScreeningResolutionRepo
{
    public interface IScreeningResolutionRepository : IGenericRepository<ScreeningResolution, Guid, AppDbContext>
    {
        Task<ScreeningResolution?> GetByProcessAndPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default);

        Task<List<ScreeningResolution>> GetByProcessAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default);

        Task<int> CountByProcessAndDecisionAsync(
            Guid studySelectionProcessId,
            ScreeningDecisionType decision,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns paper IDs that have a resolution in the specified phase with the specified decision.
        /// Used for phase progression: e.g., get papers included in TitleAbstract → eligible for FullText.
        /// </summary>
        Task<List<Guid>> GetResolvedPaperIdsByPhaseAsync(
            Guid studySelectionProcessId,
            ScreeningPhase phase,
            ScreeningDecisionType decision,
            CancellationToken cancellationToken = default);
    }
}
