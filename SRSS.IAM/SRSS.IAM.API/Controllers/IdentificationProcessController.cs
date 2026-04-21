using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.IdentificationService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Identification Process lifecycle
    /// </summary>
    [ApiController]
    [Route("api")]
    public class IdentificationProcessController : BaseController
    {
        private readonly IIdentificationService _identificationService;

        public IdentificationProcessController(IIdentificationService identificationService)
        {
            _identificationService = identificationService;
        }

        /// <summary>
        /// Create a new Identification Process for a Review Process
        /// </summary>
        /// <param name="reviewProcessId">Review Process ID</param>
        /// <param name="request">Creation request with notes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Identification Process details</returns>
        [HttpPost("review-processes/{reviewProcessId}/identification")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> CreateIdentificationProcess(
            [FromRoute] Guid reviewProcessId,
            [FromBody] CreateIdentificationProcessRequest request,
            CancellationToken cancellationToken)
        {
            request.ReviewProcessId = reviewProcessId;
            var result = await _identificationService.CreateIdentificationProcessAsync(request, cancellationToken);
            return Created(result, "Identification Process created successfully.");
        }

        /// <summary>
        /// Get Identification Process by ID
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Identification Process details</returns>
        [HttpGet("identification-processes/{id}")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> GetIdentificationProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.GetIdentificationProcessByIdAsync(id, cancellationToken);
            return Ok(result, "Identification Process retrieved successfully.");
        }

        /// <summary>
        /// Start Identification Process (transitions from NotStarted to InProgress)
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated Identification Process details</returns>
        [HttpPost("identification-processes/{id}/start")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> StartIdentificationProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.StartIdentificationProcessAsync(id, cancellationToken);
            return Ok(result, "Identification Process started successfully.");
        }

        /// <summary>
        /// Complete Identification Process (transitions from InProgress to Completed)
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated Identification Process details</returns>
        [HttpPost("identification-processes/{id}/complete")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> CompleteIdentificationProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.CompleteIdentificationProcessAsync(id, cancellationToken);
            return Ok(result, "Identification Process completed successfully.");
        }

        /// <summary>
        /// Reopen Identification Process (transitions from Completed to InProgress)
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated Identification Process details</returns>
        [HttpPost("identification-processes/{id}/reopen")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> ReopenIdentificationProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.ReopenIdentificationProcessAsync(id, cancellationToken);
            return Ok(result, "Identification Process reopened successfully.");
        }

        /// <summary>
        /// Get PRISMA statistics for an Identification Process
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Aggregated PRISMA statistics including import counts and deduplication metrics</returns>
        [HttpGet("identification-processes/{id}/statistics")]
        public async Task<ActionResult<ApiResponse<PrismaStatisticsResponse>>> GetPrismaStatistics(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.GetPrismaStatisticsAsync(id, cancellationToken);
            return Ok(result, "PRISMA statistics retrieved successfully.");
        }

        /// <summary>
        /// Manually mark a paper as a duplicate of another paper.
        /// </summary>
        /// <param name="identificationProcessId">ID of the identification process</param>
        /// <param name="paperId">ID of the paper to be cancelled (the duplicate)</param>
        /// <param name="request">Request containing the original paper ID and reason</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success response</returns>
        [HttpPost("identification-processes/{identificationProcessId}/papers/{paperId}/mark-as-duplicate")]
        public async Task<ActionResult<ApiResponse>> MarkAsDuplicate(
            [FromRoute] Guid identificationProcessId,
            [FromRoute] Guid paperId,
            [FromBody] MarkAsDuplicateRequest request,
            CancellationToken cancellationToken)
        {
            await _identificationService.MarkAsDuplicateAsync(identificationProcessId, paperId, request, cancellationToken);
            return Ok("Paper marked as duplicate successfully.");
        }

        /// <summary>
        /// Get papers that are ready to be added to the snapshot.
        /// Excludes duplicates, pending papers, and papers already in the snapshot.
        /// </summary>
        [HttpGet("identification-processes/{id}/ready-papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetReadyPapers(
            [FromRoute] Guid id,
            [FromQuery] SnapshotPaperQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var (papers, totalCount) = await _identificationService.GetReadyPapersForSnapshotAsync(
                id,
                request.Search,
                request.Year,
                request.SearchSourceId,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
            
            var result = new PaginatedResponse<PaperResponse>
            {
                Items = papers,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            return Ok(result, "Ready papers retrieved successfully.");
        }

        /// <summary>
        /// Bulk add papers to the identification snapshot.
        /// Snapshot is append-only.
        /// </summary>
        // [HttpPost("identification-processes/{id}/snapshot")]
        // public async Task<ActionResult<ApiResponse>> AddPapersToSnapshot(
        //     [FromRoute] Guid id,
        //     [FromBody] AddPapersToSnapshotRequest request,
        //     CancellationToken cancellationToken)
        // {
        //     await _identificationService.AddPapersToIdentificationSnapshotAsync(id, request.PaperIds, cancellationToken);
        //     return Ok("Papers added to snapshot successfully.");
        // }

        /// <summary>
        /// Get the frozen identification snapshot papers.
        /// </summary>
        [HttpGet("identification-processes/{id}/snapshot")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetSnapshot(
            [FromRoute] Guid id,
            [FromQuery] SnapshotPaperQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var (papers, totalCount) = await _identificationService.GetPaperIdentificationProcessSnapshotAsync(
                id,
                request.Search,
                request.Year,
                request.SearchSourceId,
                request.PageNumber,
                request.PageSize,
                cancellationToken);
            
            var result = new PaginatedResponse<PaperResponse>
            {
                Items = papers,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            return Ok(result, "Snapshot papers retrieved successfully.");
        }
    }
}
