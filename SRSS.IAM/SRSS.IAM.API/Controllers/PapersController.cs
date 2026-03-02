using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.PaperService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Papers in Systematic Review Projects
    /// </summary>
    [ApiController]
    [Route("api")]
    public class PapersController : BaseController
    {
        private readonly IPaperService _paperService;

        public PapersController(IPaperService paperService)
        {
            _paperService = paperService;
        }

        /// <summary>
        /// Get all papers for a specific project with optional filtering and pagination
        /// </summary>
        [HttpGet("projects/{projectId}/papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetPapersByProject(
            [FromRoute] Guid projectId,
            [FromQuery] string? search,
            [FromQuery] SelectionStatus? status,
            [FromQuery] int? year,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new PaperListRequest
            {
                Search = search,
                Status = status,
                Year = year,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _paperService.GetPapersByProjectAsync(projectId, request, cancellationToken);

            var message = result.TotalCount == 0
                ? "No papers found for this project."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} papers.";

            return Ok(result, message);
        }

        /// <summary>
        /// Get all duplicate papers for a specific identification process
        /// Uses process-scoped deduplication results (not project-wide)
        /// </summary>
        /// <param name="identificationProcessId">Identification Process ID</param>
        /// <param name="search">Search in Title, DOI, or Authors</param>
        /// <param name="year">Filter by publication year</param>
        /// <param name="sortBy">Sort field: detectedAt (default), confidenceScore, title, method, reviewStatus</param>
        /// <param name="sortOrder">Sort direction: asc or desc (default)</param>
        /// <param name="reviewStatus">Filter by review status: 0=Pending, 1=Confirmed, 2=Rejected</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of duplicate papers with deduplication metadata</returns>
        [HttpGet("identification-processes/{identificationProcessId}/duplicates")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DuplicatePaperResponse>>>> GetDuplicatePapersByIdentificationProcess(
            [FromRoute] Guid identificationProcessId,
            [FromQuery] string? search,
            [FromQuery] int? year,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] DeduplicationReviewStatus? reviewStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new DuplicatePapersRequest
            {
                Search = search,
                Year = year,
                SortBy = sortBy,
                SortOrder = sortOrder,
                ReviewStatus = reviewStatus,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _paperService.GetDuplicatePapersByIdentificationProcessAsync(
                identificationProcessId, 
                request, 
                cancellationToken);

            var message = result.TotalCount == 0
                ? "No duplicate papers found for this identification process."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} duplicate papers.";

            return Ok(result, message);
        }

        /// <summary>
        /// Resolve a duplicate detection result (confirm as duplicate or reject)
        /// </summary>
        /// <param name="identificationProcessId">Identification Process ID</param>
        /// <param name="deduplicationResultId">Deduplication Result ID</param>
        /// <param name="request">Resolution details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated duplicate paper with resolution status</returns>
        [HttpPost("identification-processes/{identificationProcessId}/duplicates/{deduplicationResultId}/resolve")]
        public async Task<ActionResult<ApiResponse<DuplicatePaperResponse>>> ResolveDuplicate(
            [FromRoute] Guid identificationProcessId,
            [FromRoute] Guid deduplicationResultId,
            [FromBody] ResolveDuplicateRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.ResolveDuplicateAsync(
                identificationProcessId,
                deduplicationResultId,
                request,
                cancellationToken);

            return Ok(result, "Duplicate resolution updated successfully.");
        }

        /// <summary>
        /// Get unique (non-duplicate) papers for a specific identification process.
        /// Returns papers imported via this process that are NOT marked as duplicates.
        /// </summary>
        [HttpGet("identification-processes/{identificationProcessId}/unique-papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetUniquePapersByIdentificationProcess(
            [FromRoute] Guid identificationProcessId,
            [FromQuery] string? search,
            [FromQuery] int? year,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new PaperListRequest
            {
                Search = search,
                Year = year,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _paperService.GetUniquePapersByIdentificationProcessAsync(
                identificationProcessId,
                request,
                cancellationToken);

            var message = result.TotalCount == 0
                ? "No unique papers found for this identification process."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} unique papers.";

            return Ok(result, message);
        }

        /// <summary>
        /// Get paginated duplicate pairs with both papers for side-by-side comparison.
        /// Each pair contains the original paper and the duplicate paper with full metadata.
        /// </summary>
        /// <param name="identificationProcessId">Identification Process ID</param>
        /// <param name="search">Search in title/DOI/authors of either paper in the pair</param>
        /// <param name="status">Filter by review status: 0=Pending, 1=Confirmed, 2=Rejected</param>
        /// <param name="minConfidence">Filter pairs with confidence >= value (0.0 to 1.0)</param>
        /// <param name="method">Filter by detection method: 0=DOI_MATCH, 1=TITLE_FUZZY, 2=TITLE_AUTHOR, 3=HYBRID, 4=MANUAL</param>
        /// <param name="sortBy">Sort order: confidenceDesc (default), confidenceAsc, detectedAtDesc</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of duplicate pairs</returns>
        [HttpGet("identification-processes/{identificationProcessId}/duplicate-pairs")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DuplicatePairResponse>>>> GetDuplicatePairs(
            [FromRoute] Guid identificationProcessId,
            [FromQuery] string? search,
            [FromQuery] DeduplicationReviewStatus? status,
            [FromQuery] decimal? minConfidence,
            [FromQuery] DeduplicationMethod? method,
            [FromQuery] string? sortBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new DuplicatePairsRequest
            {
                Search = search,
                Status = status,
                MinConfidence = minConfidence,
                Method = method,
                SortBy = sortBy,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _paperService.GetDuplicatePairsAsync(
                identificationProcessId,
                request,
                cancellationToken);

            var message = result.TotalCount == 0
                ? "No duplicate pairs found for this identification process."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} duplicate pairs.";

            return Ok(result, message);
        }

        /// <summary>
        /// Resolve a duplicate pair with a specific decision.
        /// </summary>
        /// <param name="identificationProcessId">Identification Process ID</param>
        /// <param name="pairId">Deduplication Result ID (the pair ID)</param>
        /// <param name="request">Resolution decision: "keep-original", "keep-duplicate", or "keep-both"</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Resolution result with audit trail</returns>
        [HttpPatch("identification-processes/{identificationProcessId}/duplicate-pairs/{pairId}/resolve")]
        public async Task<ActionResult<ApiResponse<ResolveDuplicatePairResponse>>> ResolveDuplicatePair(
            [FromRoute] Guid identificationProcessId,
            [FromRoute] Guid pairId,
            [FromBody] ResolveDuplicatePairRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.ResolveDuplicatePairAsync(
                identificationProcessId,
                pairId,
                request,
                cancellationToken);

            return Ok(result, "Duplicate pair resolved successfully.");
        }
    }
}
