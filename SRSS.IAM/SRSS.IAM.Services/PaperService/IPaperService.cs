using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.PaperService
{
    public interface IPaperService
    {
        Task<PaginatedResponse<PaperResponse>> GetPapersByProjectAsync(
            Guid projectId,
            PaperListRequest request,
            CancellationToken cancellationToken = default);

        Task<PaperDetailsResponse> GetPaperByIdAsync(
            Guid paperId,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperResponse>> GetPaperPoolAsync(
            Guid projectId,
            PaperPoolQueryRequest request,
            CancellationToken cancellationToken = default);

        Task<PaperPoolFilterMetadataResponse> GetFilterMetadataAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<List<FilterSettingResponse>> GetFilterSettingsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<FilterSettingResponse> GetFilterSettingByIdAsync(
            Guid projectId,
            Guid id,
            CancellationToken cancellationToken = default);

        Task<FilterSettingResponse> CreateFilterSettingAsync(
            Guid projectId,
            FilterSettingRequest request,
            CancellationToken cancellationToken = default);

        Task<FilterSettingResponse> UpdateFilterSettingAsync(
            Guid projectId,
            Guid id,
            FilterSettingRequest request,
            CancellationToken cancellationToken = default);

        Task DeleteFilterSettingAsync(
            Guid projectId,
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get papers for a project with advanced search and filtering capabilities
        /// Supports filtering by search query, search strategy, search source, and publication year
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="query">Search query object containing search text, strategy ID, source ID, and year filters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of papers matching the search criteria</returns>
        Task<PaginatedResponse<PaperResponse>> SearchPapersByProjectAsync(
            Guid projectId,
            PaperSearchQuery query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get duplicate papers for a specific project
        /// Returns papers identified as duplicates in that project with deduplication metadata
        /// </summary>
        Task<PaginatedResponse<DuplicatePaperResponse>> GetDuplicatePapersByProjectAsync(
            Guid projectId,
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

        /// <summary>
        /// Get unique papers for a specific data extraction process
        /// Returns papers that have an extraction paper task in this process
        /// </summary>
        Task<PaginatedResponse<PaperResponse>> GetUniquePapersByDataExtractionProcessAsync(
            Guid dataExtractionProcessId,
            PaperListRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolve a duplicate detection result (confirm, reject)
        /// </summary>
        Task<DuplicatePaperResponse> ResolveDuplicateAsync(
            Guid projectId,
            Guid deduplicationResultId,
            ResolveDuplicateRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated duplicate pairs with both papers for side-by-side comparison
        /// </summary>
        Task<PaginatedResponse<DuplicatePairResponse>> GetDuplicatePairsAsync(
            Guid projectId,
            DuplicatePairsRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolve a duplicate pair with a specific decision (keep-original, keep-duplicate, keep-both)
        /// </summary>
        Task<ResolveDuplicatePairResponse> ResolveDuplicatePairAsync(
            Guid projectId,
            Guid pairId,
            ResolveDuplicatePairRequest request,
            CancellationToken cancellationToken = default);

        Task MarkAsDuplicateAsync(
            Guid projectId,
            Guid paperId,
            MarkAsDuplicateRequest request,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperResponse>> SearchPapersAsync(
            Guid projectId,
            PaperSearchRequest request,
            CancellationToken cancellationToken = default);

        Task AssignPapersAsync(
            AssignPapersRequest request,
            CancellationToken cancellationToken = default);

        Task<PaperResponse> ApplyMetadataAsync(
            Guid paperId,
            ApplyMetadataRequest request,
            CancellationToken cancellationToken = default);

        Task<SimplifiedPapersResponse> GetTitleAbstractEligiblePapersAsync(
            Guid studySelectionProcessId,
            EligiblePapersRequest request,
            CancellationToken cancellationToken = default);

        Task<SimplifiedPapersResponse> GetFullTextEligiblePapersAsync(
            Guid studySelectionProcessId,
            EligiblePapersRequest request,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperResponse>> GetAssignedPapersByPhaseAsync(
            Guid studySelectionProcessId,
            Guid userId,
            ScreeningPhase phase,
            PaperListRequest request,
            CancellationToken cancellationToken = default);

    }
}
