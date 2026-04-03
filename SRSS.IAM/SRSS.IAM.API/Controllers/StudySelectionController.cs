using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.StudySelectionService;
using SRSS.IAM.Services.PaperService;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Study Selection (Screening) Process
    /// </summary>
    [ApiController]
    [Route("api")]
    public class StudySelectionController : BaseController
    {
        private readonly IStudySelectionService _studySelectionService;
        private readonly IPaperService _paperService;
        private readonly ICurrentUserService _currentUserService;

        public StudySelectionController(IStudySelectionService studySelectionService, IPaperService paperService, ICurrentUserService currentUserService)
        {
            _studySelectionService = studySelectionService;
            _paperService = paperService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Create a new Study Selection Process for a Review Process
        /// </summary>
        [HttpPost("review-processes/{reviewProcessId}/study-selection")]
        public async Task<ActionResult<ApiResponse<StudySelectionProcessResponse>>> CreateStudySelectionProcess(
            [FromRoute] Guid reviewProcessId,
            [FromBody] CreateStudySelectionProcessRequest request,
            CancellationToken cancellationToken)
        {
            request.ReviewProcessId = reviewProcessId;
            var result = await _studySelectionService.CreateStudySelectionProcessAsync(request, cancellationToken);
            return Created(result, "Study Selection Process created successfully.");
        }

        /// <summary>
        /// Get Study Selection Process by ID
        /// </summary>
        [HttpGet("study-selection/{id}")]
        public async Task<ActionResult<ApiResponse<StudySelectionProcessResponse>>> GetStudySelectionProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetStudySelectionProcessAsync(id, cancellationToken);
            return Ok(result, "Study Selection Process retrieved successfully.");
        }

        /// <summary>
        /// Start Study Selection Process
        /// </summary>
        [HttpPost("study-selection/{id}/start")]
        public async Task<ActionResult<ApiResponse<StudySelectionProcessResponse>>> StartStudySelectionProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.StartStudySelectionProcessAsync(id, cancellationToken);
            return Ok(result, "Study Selection Process started successfully.");
        }

        /// <summary>
        /// Complete Study Selection Process (validates no unresolved conflicts)
        /// </summary>
        [HttpPost("study-selection/{id}/complete")]
        public async Task<ActionResult<ApiResponse<StudySelectionProcessResponse>>> CompleteStudySelectionProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.CompleteStudySelectionProcessAsync(id, cancellationToken);
            return Ok(result, "Study Selection Process completed successfully.");
        }

        /// <summary>
        /// Get eligible papers for selection (excludes duplicates)
        /// </summary>
        [HttpGet("study-selection/{id}/eligible-papers")]
        public async Task<ActionResult<ApiResponse<List<Guid>>>> GetEligiblePapers(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetEligiblePapersAsync(id, cancellationToken);
            return Ok(result, $"Retrieved {result.Count} eligible papers.");
        }

        /// <summary>
        /// Submit a screening decision for a paper (one per reviewer)
        /// </summary>
        [HttpPost("study-selection/{id}/papers/{paperId}/decision")]
        public async Task<ActionResult<ApiResponse<ScreeningDecisionResponse>>> SubmitScreeningDecision(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            [FromBody] SubmitScreeningDecisionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.SubmitScreeningDecisionAsync(id, paperId, request, cancellationToken);
            return Created(result, "Screening decision submitted successfully.");
        }

        /// <summary>
        /// Get all screening decisions for a specific paper
        /// </summary>
        [HttpGet("study-selection/{id}/papers/{paperId}/decisions")]
        public async Task<ActionResult<ApiResponse<List<ScreeningDecisionResponse>>>> GetDecisionsByPaper(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetDecisionsByPaperAsync(id, paperId, cancellationToken);
            return Ok(result, $"Retrieved {result.Count} decisions.");
        }

        /// <summary>
        /// Get papers with conflicting decisions (Include vs Exclude)
        /// </summary>
        [HttpGet("study-selection/{id}/conflicts")]
        public async Task<ActionResult<ApiResponse<List<ConflictedPaperResponse>>>> GetConflictedPapers(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetConflictedPapersAsync(id, cancellationToken);
            return Ok(result, $"Found {result.Count} conflicted papers.");
        }

        /// <summary>
        /// Get papers with conflicting decisions grouped by phase (Title/Abstract and Full Text)
        /// Supports pagination, filtering by status, and search by title/authors/doi
        /// </summary>
        [HttpGet("study-selection/{id}/conflicts-by-phase")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PhaseConflictedPaperResponse>>>> GetConflictedPapersByPhase(
            [FromRoute] Guid id,
            [FromQuery] ScreeningPhase? phase,
            [FromQuery] PaperSelectionStatus? status,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new ConflictedPapersRequest
            {
                Phase = phase,
                Status = status,
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _studySelectionService.GetConflictedPapersByPhaseAsync(id, request, cancellationToken);

            var message = result.TotalCount == 0
                ? "No conflicted papers found matching the criteria."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} conflicted papers (page {result.PageNumber}).";

            return Ok(result, message);
        }

        /// <summary>
        /// Get detailed information of a conflicted paper for resolution
        /// Includes core metadata, metadata sources, decisions, and full-text links.
        /// Phase TitleAbstract: includes Abstract and Keywords.
        /// Phase FullText: includes Full-Text access links.
        /// </summary>
        [HttpGet("study-selection/{id}/papers/{paperId}/conflict-detail")]
        public async Task<ActionResult<ApiResponse<ConflictPaperDetailResponse>>> GetConflictPaperDetail(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetConflictPaperDetailAsync(id, paperId, phase, cancellationToken);
            return Ok(result, "Conflicted paper detail retrieved successfully.");
        }

        /// <summary>
        /// Resolve a conflicted paper with final decision
        /// </summary>
        [HttpPost("study-selection/{id}/papers/{paperId}/resolve")]
        public async Task<ActionResult<ApiResponse<ScreeningResolutionResponse>>> ResolveConflict(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            [FromBody] ResolveScreeningConflictRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.ResolveConflictAsync(id, paperId, request, cancellationToken);
            return Created(result, "Conflict resolved successfully.");
        }

        /// <summary>
        /// Get all resolution papers for a specific phase or all phases (paginated, with filter/search)
        /// Includes more paper metadata: Title, Authors, DOI, Year, Source.
        /// </summary>
        [HttpGet("study-selection/{id}/resolutions")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ScreeningResolutionPaperResponse>>>> GetResolutions(
            [FromRoute] Guid id,
            [FromQuery] ScreeningPhase? phase,
            [FromQuery] ScreeningDecisionType? finalDecision,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new GetResolutionsRequest
            {
                Phase = phase,
                FinalDecision = finalDecision,
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _studySelectionService.GetResolutionsAsync(id, request, cancellationToken);
            
            var message = result.TotalCount == 0
                ? "No resolution papers found matching the criteria."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} resolution papers (page {result.PageNumber}).";

            return Ok(result, message);
        }

        /// <summary>
        /// Get selection status for a specific paper
        /// </summary>
        [HttpGet("study-selection/{id}/papers/{paperId}/status")]
        public async Task<ActionResult<ApiResponse<PaperSelectionStatus>>> GetPaperSelectionStatus(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetPaperSelectionStatusAsync(id, paperId, cancellationToken);
            return Ok(result, "Paper selection status retrieved successfully.");
        }

        /// <summary>
        /// Get selection statistics (for PRISMA reporting)
        /// </summary>
        [HttpGet("study-selection/{id}/statistics")]
        public async Task<ActionResult<ApiResponse<SelectionStatisticsResponse>>> GetSelectionStatistics(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetSelectionStatisticsAsync(id, cancellationToken);
            return Ok(result, "Selection statistics retrieved successfully.");
        }

        /// <summary>
        /// Get the current screening phase status of a Study Selection Process
        /// </summary>
        [HttpGet("study-selection/{studySelectionProcessId}/phase-status")]
        public async Task<ActionResult<ApiResponse<StudySelectionPhaseStatusResponse>>> GetPhaseStatus(
            [FromRoute] Guid studySelectionProcessId,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetPhaseStatusAsync(studySelectionProcessId, cancellationToken);
            return Ok(result, "Study Selection phase status retrieved successfully.");
        }

        /// <summary>
        /// Get all papers with their decisions and resolutions (paginated, with search/filter/sort)
        /// </summary>
        [HttpGet("study-selection/{id}/papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperWithDecisionsResponse>>>> GetPapersWithDecisions(
            [FromRoute] Guid id,
            [FromQuery] string? search,
            [FromQuery] PaperSelectionStatus? status,
            [FromQuery] PaperSortBy sortBy = PaperSortBy.TitleAsc,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? hasFullText = null,
            [FromQuery] bool? hasConflict = null,
            [FromQuery] Guid? decidedByReviewerId = null,
            [FromQuery] ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var request = new PapersWithDecisionsRequest
            {
                Search = search,
                Status = status,
                SortBy = sortBy,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasFullText = hasFullText,
                HasConflict = hasConflict,
                DecidedByReviewerId = decidedByReviewerId,
                Phase = phase
            };

            var result = await _studySelectionService.GetPapersWithDecisionsAsync(id, request, cancellationToken);

            var message = result.TotalCount == 0
                ? "No papers found matching the criteria."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} papers (page {result.PageNumber}/{result.TotalPages}).";

            return Ok(result, message);
        }

        [HttpGet("study-selection/{id}/papers/{paperId}")]
        public async Task<ActionResult<ApiResponse<PaperWithDecisionsResponse>>> GetPaperWithDecisions(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetPaperDetailsAsync(id, paperId, cancellationToken);
            return Ok(result, "Paper with decisions retrieved successfully.");
        }



        /// <summary>
        /// Get papers assigned to the current user (reviewer) (paginated, with search/filter/sort)
        /// </summary>
        [HttpGet("study-selection/{id}/assigned-papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperWithDecisionsResponse>>>> GetAssignedPapers(
            [FromRoute] Guid id,
            [FromQuery] string? search,
            [FromQuery] PaperSelectionStatus? status,
            [FromQuery] PaperSortBy sortBy = PaperSortBy.TitleAsc,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? hasFullText = null,
            [FromQuery] bool? hasConflict = null,
            [FromQuery] Guid? decidedByReviewerId = null,
            [FromQuery] ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(_currentUserService.GetUserId());
            var request = new PapersWithDecisionsRequest
            {
                Search = search,
                Status = status,
                SortBy = sortBy,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasFullText = hasFullText,
                HasConflict = hasConflict,
                DecidedByReviewerId = decidedByReviewerId,
                Phase = phase
            };

            var result = await _studySelectionService.GetAssignedPapersAsync(id, userId, request, cancellationToken);

            var message = result.TotalCount == 0
                ? "No assigned papers found matching the criteria."
                : $"Retrieved {result.Items.Count} of {result.TotalCount} assigned papers (page {result.PageNumber}/{result.TotalPages}).";

            return Ok(result, message);
        }


        /// <summary>
        /// Update full-text link (PDF URL / web URL) for a paper (Issue 2)
        /// </summary>
        [HttpPost("study-selection/{id}/papers/{paperId}/full-text")]
        public async Task<ActionResult<ApiResponse<PaperWithDecisionsResponse>>> UpdatePaperFullText(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            [FromBody] UpdatePaperFullTextRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.UpdatePaperFullTextAsync(id, paperId, request, cancellationToken);
            return Ok(result, "Paper full-text updated successfully.");
        }

        // ============================================
        // Title-Abstract Screening Lifecycle
        // ============================================

        /// <summary>
        /// Create Title/Abstract Screening phase for a Study Selection Process
        /// </summary>
        [HttpPost("study-selection/{id}/title-abstract")]
        public async Task<ActionResult<ApiResponse<TitleAbstractScreeningResponse>>> CreateTitleAbstractScreening(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.CreateTitleAbstractScreeningAsync(id, cancellationToken);
            return Created(result, "Title/Abstract screening created successfully.");
        }

        /// <summary>
        /// Start Title/Abstract Screening (validates protocol, locks protocol, validates paper metadata)
        /// </summary>
        [HttpPost("study-selection/{id}/title-abstract/start")]
        public async Task<ActionResult<ApiResponse<TitleAbstractScreeningResponse>>> StartTitleAbstractScreening(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.StartTitleAbstractScreeningAsync(id, cancellationToken);
            return Ok(result, "Title/Abstract screening started successfully.");
        }

        /// <summary>
        /// Complete Title/Abstract Screening (validates no unresolved conflicts)
        /// </summary>
        [HttpPost("study-selection/{id}/title-abstract/complete")]
        public async Task<ActionResult<ApiResponse<TitleAbstractScreeningResponse>>> CompleteTitleAbstractScreening(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.CompleteTitleAbstractScreeningAsync(id, cancellationToken);
            return Ok(result, "Title/Abstract screening completed successfully.");
        }

        /// <summary>
        /// Get Title/Abstract Screening status
        /// </summary>
        [HttpGet("study-selection/{id}/title-abstract")]
        public async Task<ActionResult<ApiResponse<TitleAbstractScreeningResponse>>> GetTitleAbstractScreening(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetTitleAbstractScreeningAsync(id, cancellationToken);
            return Ok(result, "Title/Abstract screening retrieved successfully.");
        }

        /// <summary>
        /// Get papers eligible for Title/Abstract screening (Step 1)
        /// </summary>
        [HttpGet("study-selection/{studySelectionProcessId}/title-abstract/papers")]
        public async Task<ActionResult<ApiResponse<CheckedDuplicatePapersResponse>>> GetTitleAbstractEligiblePapers(
            [FromRoute] Guid studySelectionProcessId,
            [FromQuery] CheckedDuplicatePapersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.GetTitleAbstractEligiblePapersAsync(studySelectionProcessId, request, cancellationToken);
            return Ok(result, "Title/Abstract eligible papers retrieved successfully.");
        }

        /// <summary>
        /// Get papers eligible for Full-Text screening (Step 2)
        /// </summary>
        [HttpGet("study-selection/{studySelectionProcessId}/full-text/papers")]
        public async Task<ActionResult<ApiResponse<CheckedDuplicatePapersResponse>>> GetFullTextEligiblePapers(
            [FromRoute] Guid studySelectionProcessId,
            [FromQuery] CheckedDuplicatePapersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.GetFullTextEligiblePapersAsync(studySelectionProcessId, request, cancellationToken);
            return Ok(result, "Full-Text eligible papers retrieved successfully.");
        }
    }
}
