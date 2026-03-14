using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.PaperService
{
    public interface IPaperService
    {
        Task<PaginatedResponse<PaperResponse>> GetPapersByProjectAsync(
            Guid projectId,
            PaperListRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get duplicate papers for a specific identification process
        /// Returns papers identified as duplicates in that process with deduplication metadata
        /// </summary>
        Task<PaginatedResponse<DuplicatePaperResponse>> GetDuplicatePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            DuplicatePapersRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get unique (non-duplicate) papers for a specific identification process
        /// Returns papers that have no deduplication results in that process
        /// </summary>
        Task<PaginatedResponse<PaperResponse>> GetUniquePapersByIdentificationProcessAsync(
            Guid identificationProcessId,
            PaperListRequest request,
            CancellationToken cancellationToken = default);

        Task<PaperResponse> GetPaperByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolve a duplicate detection result (confirm, reject)
        /// </summary>
        Task<DuplicatePaperResponse> ResolveDuplicateAsync(
            Guid identificationProcessId,
            Guid deduplicationResultId,
            ResolveDuplicateRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated duplicate pairs with both papers for side-by-side comparison
        /// </summary>
        Task<PaginatedResponse<DuplicatePairResponse>> GetDuplicatePairsAsync(
            Guid identificationProcessId,
            DuplicatePairsRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolve a duplicate pair with a specific decision (keep-original, keep-duplicate, keep-both)
        /// </summary>
        Task<ResolveDuplicatePairResponse> ResolveDuplicatePairAsync(
            Guid identificationProcessId,
            Guid pairId,
            ResolveDuplicatePairRequest request,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperResponse>> SearchPapersAsync(
            Guid projectId,
            PaperSearchRequest request,
            CancellationToken cancellationToken = default);
    }
}
