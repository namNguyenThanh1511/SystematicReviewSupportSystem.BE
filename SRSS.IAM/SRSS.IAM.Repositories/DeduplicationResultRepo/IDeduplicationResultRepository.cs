using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DeduplicationResultRepo
{
    public interface IDeduplicationResultRepository : IGenericRepository<DeduplicationResult, Guid, AppDbContext>
    {
        Task<List<DeduplicationResult>> GetByIdentificationProcessAsync(
            Guid identificationProcessId, 
            CancellationToken cancellationToken = default);

        Task<DeduplicationResult?> GetByPaperAndProcessAsync(
            Guid paperId, 
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);

        Task<int> CountDuplicatesByProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated duplicate pairs with both papers included for side-by-side comparison.
        /// </summary>
        Task<(List<DeduplicationResult> Results, int TotalCount)> GetDuplicatePairsAsync(
            Guid identificationProcessId,
            string? search,
            DeduplicationReviewStatus? status,
            decimal? minConfidence,
            DeduplicationMethod? method,
            string? sortBy,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
