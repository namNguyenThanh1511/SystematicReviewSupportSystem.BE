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

        Task<PaginatedResponse<PaperResponse>> SearchPapersAsync(
            Guid projectId,
            PaperSearchRequest request,
            CancellationToken cancellationToken = default);
    }
}
