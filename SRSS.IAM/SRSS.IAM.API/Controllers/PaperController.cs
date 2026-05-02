using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.IdentificationService;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.PaperService;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/papers")]
    public class PaperController : BaseController
    {
        private readonly IIdentificationService _identificationService;
        private readonly IPaperService _paperService;
        private readonly ICurrentUserService _currentUserService;

        public PaperController(IIdentificationService identificationService, IPaperService paperService, ICurrentUserService currentUserService)
        {
            _identificationService = identificationService;
            _paperService = paperService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Import bibliographic records from a RIS file
        /// </summary>
        /// <param name="file">RIS file (.ris extension)</param>
        /// <param name="searchSourceId">Source database ID (referencing SearchSource entity)</param>
        /// <param name="importedBy">User who performed the import</param>
        /// <param name="searchStrategyId">Optional search strategy ID used for this import</param>
        /// <param name="projectId">Project ID that owns the paper pool</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Import summary with counts and any errors</returns>
        [HttpPost("import/ris")]
        public async Task<ActionResult<ApiResponse<RisImportResultDto>>> ImportRisFile(
            IFormFile file,
            [FromForm] Guid? searchSourceId,
            [FromForm] Guid projectId,
            CancellationToken cancellationToken)
        {
            // Validate file presence
            if (file == null || file.Length == 0)
            {
                return BadRequest<RisImportResultDto>("No file uploaded.");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".ris")
            {
                return BadRequest<RisImportResultDto>("Invalid file format. Only .ris files are accepted.");
            }

            // Validate file size (e.g., max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                return BadRequest<RisImportResultDto>("File size exceeds the maximum allowed size of 10MB.");
            }

            using var stream = file.OpenReadStream();
            var result = await _identificationService.ImportRisFileAsync(
                stream,
                file.FileName,
                searchSourceId,
                projectId,
                cancellationToken);

            // Check if import was successful
            if (result.TotalRecords == 0 && result.Errors.Any())
            {
                return BadRequest<RisImportResultDto>("Failed to import RIS file.");
            }

            if (result.ImportedRecords == 0 && result.UpdatedRecords == 0)
            {
                return Ok(result, "No new records imported. All records were duplicates or skipped.");
            }

            return Ok(result, $"Successfully imported {result.ImportedRecords} records.");
        }

        /// <summary>
        /// Import a single paper by resolving its DOI via Crossref
        /// </summary>
        [HttpPost("import/doi")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RisImportResultDto>>> ImportFromDoi(
            [FromBody] DoiImportRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.ImportFromDoiAsync(
                request.Doi,
                request.SearchSourceId,
                request.ProjectId,
                cancellationToken);

            return Ok(result, "Successfully imported record from DOI.");
        }

        /// <summary>
        /// Import multiple papers by querying Crossref API
        /// </summary>
        [HttpPost("import/cross-ref")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RisImportResultDto>>> ImportFromApi(
            [FromBody] ApiImportRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.ImportFromApiAsync(
                request.Query,
                request.SearchSourceId,
                request.ProjectId,
                cancellationToken);

            return Ok(result, $"Successfully imported {result.ImportedRecords} records from API.");
        }


        /// <summary>
        /// Assign single/multiple papers to single/multiple project members.
        /// Prevents duplicate assignments and ensures both papers and members belong to the project.
        /// Project ID is inferred from the papers.
        /// </summary>
        /// <param name="request">Paper IDs and Member IDs to assign</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPost("assign")]
        public async Task<ActionResult<ApiResponse>> AssignPapers(
            [FromBody] AssignPapersRequest request,
            CancellationToken cancellationToken)
        {
            await _paperService.AssignPapersAsync(request, cancellationToken);
            return Ok("Papers assigned successfully.");
        }

        /// <summary>
        /// Apply selected metadata fields from a metadata source to the canonical paper record.
        /// </summary>
        /// <param name="paperId">The Paper ID</param>
        /// <param name="request">The selected fields and source metadata ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated PaperResponse</returns>
        [HttpPost("{paperId}/apply-metadata")]
        public async Task<ActionResult<ApiResponse<PaperResponse>>> ApplyMetadata(
            [FromRoute] Guid paperId,
            [FromBody] ApplyMetadataRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.ApplyMetadataAsync(paperId, request, cancellationToken);
            return Ok(result, "Selected metadata applied successfully.");
        }

        /// <summary>
        /// Get assigned papers for the current user in Title/Abstract screening phase
        /// </summary>
        [Authorize]
        [HttpGet("assigned/title-abstract")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetAssignedPapersTitleAbstract(
            [FromQuery] Guid studySelectionId,
            [FromQuery] PaperListRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            var result = await _paperService.GetAssignedPapersByPhaseAsync(
                studySelectionId,
                userId,
                ScreeningPhase.TitleAbstract,
                request,
                cancellationToken);
            return Ok(result, "Assigned papers for Title/Abstract screening retrieved successfully.");
        }

        /// <summary>
        /// Get assigned papers for the current user in Full-Text screening phase
        /// </summary>
        [Authorize]
        [HttpGet("assigned/full-text")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetAssignedPapersFullText(
            [FromQuery] Guid studySelectionId,
            [FromQuery] PaperListRequest request,
            CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            var result = await _paperService.GetAssignedPapersByPhaseAsync(
                studySelectionId,
                userId,
                ScreeningPhase.FullText,
                request,
                cancellationToken);
            return Ok(result, "Assigned papers for Full-Text screening retrieved successfully.");
        }

        /// <summary>
        /// Get papers for a project with advanced search and filtering
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="search">Text to search in title, abstract, authors, keywords</param>
        /// <param name="searchStrategyId">Optional search strategy ID to filter papers imported via specific strategy</param>
        /// <param name="searchSourceId">Optional search source ID (e.g., PubMed, Scopus) to filter papers from specific database</param>
        /// <param name="year">Optional publication year to filter papers</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of papers matching the search criteria</returns>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetPapersForProject(
            [FromRoute] Guid projectId,
            [FromQuery] string? search,
            [FromQuery] Guid? searchStrategyId,
            [FromQuery] Guid? searchSourceId,
            [FromQuery] int? year,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new PaperSearchQuery
            {
                Search = search,
                SearchStrategyId = searchStrategyId,
                SearchSourceId = searchSourceId,
                Year = year,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _paperService.SearchPapersByProjectAsync(
                projectId,
                query,
                cancellationToken);

            return Ok(result, $"Retrieved {result.Items.Count} papers for project.");
        }

        [HttpGet("/api/projects/{projectId}/papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperResponse>>>> GetPaperPool(
            [FromRoute] Guid projectId,
            [FromQuery] PaperPoolQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidState(request.DoiState))
            {
                return BadRequest<PaginatedResponse<PaperResponse>>("doiState must be one of: all, has, missing.");
            }

            if (!IsValidState(request.FullTextState))
            {
                return BadRequest<PaginatedResponse<PaperResponse>>("fullTextState must be one of: all, has, missing.");
            }

            if (request.YearFrom.HasValue && request.YearTo.HasValue && request.YearFrom.Value > request.YearTo.Value)
            {
                return BadRequest<PaginatedResponse<PaperResponse>>("yearFrom must be less than or equal to yearTo.");
            }

            var result = await _paperService.GetPaperPoolAsync(projectId, request, cancellationToken);
            return Ok(result, "Paper pool retrieved successfully.");
        }

        [HttpGet("/api/projects/{projectId}/filter-metadata")]
        public async Task<ActionResult<ApiResponse<PaperPoolFilterMetadataResponse>>> GetFilterMetadata(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _paperService.GetFilterMetadataAsync(projectId, cancellationToken);
            return Ok(result, "Filter metadata retrieved successfully.");
        }

        [HttpGet("/api/projects/{projectId}/filter-settings")]
        public async Task<ActionResult<ApiResponse<List<FilterSettingResponse>>>> GetFilterSettings(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await _paperService.GetFilterSettingsAsync(projectId, cancellationToken);
            return Ok(result, "Filter settings retrieved successfully.");
        }

        [HttpGet("/api/projects/{projectId}/filter-settings/{id}")]
        public async Task<ActionResult<ApiResponse<FilterSettingResponse>>> GetFilterSettingById(
            [FromRoute] Guid projectId,
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _paperService.GetFilterSettingByIdAsync(projectId, id, cancellationToken);
            return Ok(result, "Filter setting retrieved successfully.");
        }

        [HttpPost("/api/projects/{projectId}/filter-settings")]
        public async Task<ActionResult<ApiResponse<FilterSettingResponse>>> CreateFilterSetting(
            [FromRoute] Guid projectId,
            [FromBody] FilterSettingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidState(request.Filters.DoiState))
            {
                return BadRequest<FilterSettingResponse>("doiState must be one of: all, has, missing.");
            }

            if (!IsValidState(request.Filters.FullTextState))
            {
                return BadRequest<FilterSettingResponse>("fullTextState must be one of: all, has, missing.");
            }

            var result = await _paperService.CreateFilterSettingAsync(projectId, request, cancellationToken);
            return Created(result, "Filter setting created successfully.");
        }

        [HttpPut("/api/projects/{projectId}/filter-settings/{id}")]
        public async Task<ActionResult<ApiResponse<FilterSettingResponse>>> UpdateFilterSetting(
            [FromRoute] Guid projectId,
            [FromRoute] Guid id,
            [FromBody] FilterSettingRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidState(request.Filters.DoiState))
            {
                return BadRequest<FilterSettingResponse>("doiState must be one of: all, has, missing.");
            }

            if (!IsValidState(request.Filters.FullTextState))
            {
                return BadRequest<FilterSettingResponse>("fullTextState must be one of: all, has, missing.");
            }

            var result = await _paperService.UpdateFilterSettingAsync(projectId, id, request, cancellationToken);
            return Ok(result, "Filter setting updated successfully.");
        }

        [HttpDelete("/api/projects/{projectId}/filter-settings/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteFilterSetting(
            [FromRoute] Guid projectId,
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            await _paperService.DeleteFilterSettingAsync(projectId, id, cancellationToken);
            return Ok("Filter setting deleted successfully.");
        }

        private static bool IsValidState(string? value)
        {
            return string.Equals(value, "all", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "has", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet("{paperId}")]
        public async Task<ActionResult<ApiResponse<PaperDetailsResponse>>> GetPaperById(
                [FromRoute] Guid paperId,
                CancellationToken cancellationToken)
        {
            var result = await _paperService.GetPaperByIdAsync(paperId, cancellationToken);
            return Ok(result, "Paper details retrieved successfully.");
        }

        /// <summary>
        /// Soft delete a paper with a reason.
        /// </summary>
        /// <param name="paperId">The Paper ID</param>
        /// <param name="request">The delete reason</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpDelete("{paperId}")]
        public async Task<ActionResult<ApiResponse>> DeletePaper(
            [FromRoute] Guid paperId,
            [FromBody] DeletePaperRequest request,
            CancellationToken cancellationToken)
        {
            await _paperService.DeletePaperAsync(paperId, request, cancellationToken);
            return Ok("Paper deleted successfully.");
        }

        /// <summary>
        /// Get all soft-deleted papers for a project.
        /// Includes deletion reason and audit info.
        /// </summary>
        [HttpGet("/api/projects/{projectId}/deleted-papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperDetailsResponse>>>> GetDeletedPapers(
            [FromRoute] Guid projectId,
            [FromQuery] PaperListRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.GetDeletedPapersAsync(projectId, request, cancellationToken);
            return Ok(result, "Deleted papers retrieved successfully.");
        }

        /// <summary>
        /// Get all confirmed duplicate papers for a project.
        /// Includes resolution info (who resolved, when).
        /// </summary>
        [HttpGet("/api/projects/{projectId}/confirmed-duplicates")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DuplicatePaperResponse>>>> GetConfirmedDuplicatePapers(
            [FromRoute] Guid projectId,
            [FromQuery] DuplicatePapersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.GetConfirmedDuplicatePapersAsync(projectId, request, cancellationToken);
            return Ok(result, "Confirmed duplicate papers retrieved successfully.");
        }

        /// <summary>
        /// Remove PDF attachment from a paper.
        /// </summary>
        /// <param name="paperId">The Paper ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpDelete("{paperId}/pdf")]
        public async Task<ActionResult<ApiResponse>> RemovePdfAttachment(
            [FromRoute] Guid paperId,
            CancellationToken cancellationToken)
        {
            await _paperService.RemovePdfAttachmentAsync(paperId, cancellationToken);
            return Ok("PDF attachment removed successfully.");
        }
    }
}
