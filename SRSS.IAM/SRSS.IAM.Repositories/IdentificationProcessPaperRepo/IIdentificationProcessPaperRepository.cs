using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.IdentificationProcessPaperRepo
{
    public interface IIdentificationProcessPaperRepository : IGenericRepository<IdentificationProcessPaper, Guid, AppDbContext>
    {
        /// <summary>
        /// Get all paper IDs from the snapshot for a given identification process.
        /// Only returns papers where IncludedAfterDedup is true.
        /// </summary>
        Task<List<Guid>> GetIncludedPaperIdsByProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a snapshot already exists for this identification process.
        /// </summary>
        Task<bool> SnapshotExistsAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);
    }
}
