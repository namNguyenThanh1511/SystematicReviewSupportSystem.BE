using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DeduplicationResultRepo
{
    public interface IDeduplicationResultRepository : IGenericRepository<DeduplicationResult, Guid, AppDbContext>
    {
        Task<List<DeduplicationResult>> GetByProjectAsync(
            Guid projectId, 
            CancellationToken cancellationToken = default);

        Task<DeduplicationResult?> GetByPaperAndProjectAsync(
            Guid paperId, 
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<int> CountDuplicatesByProjectAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated duplicate pairs with both papers included for side-by-side comparison.
        /// </summary>
        Task<(List<DeduplicationResult> Results, int TotalCount)> GetDuplicatePairsAsync(
            Guid projectId,
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
