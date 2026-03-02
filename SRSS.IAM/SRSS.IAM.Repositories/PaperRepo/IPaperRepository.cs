using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public interface IPaperRepository : IGenericRepository<Paper, Guid, AppDbContext>
    {
        Task<Paper?> GetByDoiAndProjectAsync(string doi, Guid projectId, CancellationToken cancellationToken = default);
        Task<Paper?> GetByDoiAndSearchExecutionAsync(string doi, Guid searchExecutionId, CancellationToken cancellationToken = default);

        Task<(List<Paper> Papers, int TotalCount)> GetPapersByProjectAsync(
            Guid projectId,
            string? search,
            SelectionStatus? status,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get duplicate papers for a specific identification process
        /// Uses DeduplicationResult table for process-scoped results
        /// </summary>
        Task<(List<Paper> Papers, List<DeduplicationResult> Results, int TotalCount)> GetDuplicatePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            string? sortBy,
            string? sortOrder,
            DeduplicationReviewStatus? reviewStatus,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get unique (non-duplicate) papers for a specific identification process.
        /// Returns papers imported via this process that have no deduplication results.
        /// </summary>
        Task<(List<Paper> Papers, int TotalCount)> GetUniquePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            string? search,
            int? year,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
