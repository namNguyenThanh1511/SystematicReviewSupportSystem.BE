using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionProcessRepo
{
    public interface IStudySelectionProcessRepository : IGenericRepository<StudySelectionProcess, Guid, AppDbContext>
    {
        Task<StudySelectionProcess?> GetByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default);

        Task<StudySelectionProcess?> GetActiveByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default);

        Task<StudySelectionProcess?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> HasActiveProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default);
    }
}
