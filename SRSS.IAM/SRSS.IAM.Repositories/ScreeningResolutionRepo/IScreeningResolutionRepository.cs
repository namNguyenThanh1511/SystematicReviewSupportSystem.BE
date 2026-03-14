using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

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
    }
}
