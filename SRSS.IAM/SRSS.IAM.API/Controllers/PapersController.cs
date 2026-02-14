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
        /// <param name="projectId">Project ID</param>
        /// <param name="search">Search in Title, DOI, or Authors</param>
        /// <param name="status">Filter by selection status (Pending, Included, Excluded, Duplicate)</param>
        /// <param name="year">Filter by publication year</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of papers</returns>
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
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of duplicate papers with deduplication metadata</returns>
        [HttpGet("identification-processes/{identificationProcessId}/duplicates")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DuplicatePaperResponse>>>> GetDuplicatePapersByIdentificationProcess(
            [FromRoute] Guid identificationProcessId,
            [FromQuery] string? search,
            [FromQuery] int? year,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new DuplicatePapersRequest
            {
                Search = search,
                Year = year,
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
    }
}
