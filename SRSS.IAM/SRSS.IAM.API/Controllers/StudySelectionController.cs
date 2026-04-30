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
using SRSS.IAM.Services.StudySelectionProcessPaperService;

using SRSS.IAM.Services.StudySelectionAIService;
using SRSS.IAM.Services.StuSeExclusionCodeService;
using SRSS.IAM.Services.DTOs.StuSeExclusionCode;
using Microsoft.AspNetCore.Authorization;

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
        private readonly IStuSeAIService _stuSeAIService;
        private readonly IStudySelectionAIResultService _studySelectionAIResultService;
        private readonly IStuSeExclusionCodeService _exclusionCodeService;
        private readonly IStudySelectionProcessPaperService _studySelectionProcessPaperService;
        private readonly IStuSeFullTextAiEvaluationService _fullTextAiEvaluationService;
        private readonly IStuSeFullTextAiEvaluationQueue _fullTextAiEvaluationQueue;

        public StudySelectionController(
            IStudySelectionService studySelectionService,
            IPaperService paperService,
            ICurrentUserService currentUserService,
            IStuSeAIService stuSeAIService,
            IStudySelectionAIResultService studySelectionAIResultService,
            IStuSeExclusionCodeService exclusionCodeService,
            IStudySelectionProcessPaperService studySelectionProcessPaperService,
            IStuSeFullTextAiEvaluationService fullTextAiEvaluationService,
            IStuSeFullTextAiEvaluationQueue fullTextAiEvaluationQueue)
        {
            _studySelectionService = studySelectionService;
            _paperService = paperService;
            _currentUserService = currentUserService;
            _stuSeAIService = stuSeAIService;
            _studySelectionAIResultService = studySelectionAIResultService;
            _exclusionCodeService = exclusionCodeService;
            _studySelectionProcessPaperService = studySelectionProcessPaperService;
            _fullTextAiEvaluationService = fullTextAiEvaluationService;
            _fullTextAiEvaluationQueue = fullTextAiEvaluationQueue;
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
            if (!result.IsHaveCriteria)
            {
                return Ok(result, "Failed, Setup Study Selection Criteria first to start phase");
            }
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
        /// Get all assigned reviewers and their decisions for a specific paper and phase
        /// </summary>
        [HttpGet("study-selection/{id}/papers/{paperId}/reviewer-decisions")]
        public async Task<ActionResult<ApiResponse<List<ReviewerDecisionDetailResponse>>>> GetReviewerDecisions(
            [FromRoute] Guid id,
            [FromRoute] Guid paperId,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetReviewerDecisionsAsync(id, paperId, phase, cancellationToken);
            return Ok(result, $"Retrieved decisions for {result.Count} reviewers.");
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
        /// Get conflict statuses for all papers in a specific phase (bulk check)
        /// Optimized for polling, returns only PaperId and HasConflict.
        /// </summary>
        [HttpGet("study-selection/{id}/papers/conflict-status")]
        public async Task<ActionResult<ApiResponse<List<PaperConflictStatusResponse>>>> GetPaperConflictStatuses(
            [FromRoute] Guid id,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetPaperConflictStatusesAsync(id, phase, cancellationToken);
            return Ok(result, $"Conflict statuses retrieved for {result.Count} papers.");
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
        /// Resolve multiple conflicted papers in bulk
        /// </summary>
        [HttpPost("study-selection/{id}/papers/bulk-resolve")]
        public async Task<ActionResult<ApiResponse<List<ScreeningResolutionResponse>>>> BulkResolveConflicts(
            [FromRoute] Guid id,
            [FromBody] BulkResolveConflictsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.BulkResolveConflictsAsync(id, request, cancellationToken);
            return Ok(result, $"Successfully resolved {result.Count} papers.");
        }

        /// <summary>
        /// Get all resolution papers for a specific phase or all phases (paginated, with filter/search)
        /// Includes more paper metadata: Title, Authors, DOI, Year, Source.
        /// </summary>
        [HttpGet("study-selection/{id}/resolutions")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ScreeningResolutionPaperResponse>>>> GetResolutions(
            [FromRoute] Guid id,
            [FromQuery] ScreeningPhase? phase = null,
            [FromQuery] ResolutionFilterStatus status = ResolutionFilterStatus.All,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new GetResolutionsRequest
            {
                Phase = phase,
                Status = status,
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
        /// Get the reviewer assignment table data for a specific reviewer in a specific study selection process.
        /// Grouped by paper with status for both Title/Abstract and Full-Text phases.
        /// </summary>
        [HttpGet("study-selection/{processId}/reviewers/{reviewerId}/assignment-table")]
        public async Task<ActionResult<ApiResponse<List<ReviewerAssignmentTableItemResponse>>>> GetReviewerAssignmentTable(
            [FromRoute] Guid processId,
            [FromRoute] Guid reviewerId,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetReviewerAssignmentTableAsync(processId, reviewerId, cancellationToken);
            return Ok(result, "Reviewer assignment table retrieved successfully.");
        }


        /// <summary>
        /// Update full-text link (PDF URL / web URL) for a paper (Issue 2)
        /// </summary>
        [HttpPost("papers/{paperId}/full-text")]
        public async Task<ActionResult<ApiResponse<PaperDetailsResponse>>> UpdatePaperFullText(
            [FromRoute] Guid paperId,
            [FromBody] UpdatePaperFullTextRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.UpdatePaperFullTextAsync(paperId, request, cancellationToken);
            return Ok(result, "Paper full-text updated successfully.");
        }


        /// <summary>
        /// Save multiple included papers for FullText phase
        /// Validates papers belong to the process and have Included resolution in FullText phase
        /// </summary>
        [HttpPost("study-selection/{id}/bulk-dataset")]
        public async Task<ActionResult<ApiResponse>> SaveMultipleIncludedPapersInFullTextPhase(
            [FromRoute] Guid id,
            [FromBody] SaveMultipleIncludedPapersRequest request,
            CancellationToken cancellationToken)
        {
            await _studySelectionProcessPaperService.SaveMultipleIncludedPapersInFullTextPhaseAsync(id, request.PaperIds, cancellationToken);
            return Ok("Included papers saved successfully.");
        }

        /// <summary>
        /// Get all papers with Included resolution in FullText phase (Dataset view)
        /// Responses only specific fields: title, author, year, domain, abstract description
        /// </summary>
        [HttpGet("study-selection/{id}/included-full-text-papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DatasetPaperResponse>>>> GetIncludedFullTextPapers(
            [FromRoute] Guid id,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var request = new GetResolutionsRequest
            {
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _studySelectionService.GetIncludedFullTextPapersAsync(id, request, cancellationToken);
            return Ok(result, $"Retrieved {result.Items.Count} included papers for dataset.");
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
        public async Task<ActionResult<ApiResponse<SimplifiedPapersResponse>>> GetTitleAbstractEligiblePapers(
            [FromRoute] Guid studySelectionProcessId,
            [FromQuery] EligiblePapersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.GetTitleAbstractEligiblePapersAsync(studySelectionProcessId, request, cancellationToken);
            return Ok(result, "Title/Abstract eligible papers retrieved successfully.");
        }

        /// <summary>
        /// Get papers eligible for Full-Text screening (Step 2)
        /// </summary>
        [HttpGet("study-selection/{studySelectionProcessId}/full-text/papers")]
        public async Task<ActionResult<ApiResponse<SimplifiedPapersResponse>>> GetFullTextEligiblePapers(
            [FromRoute] Guid studySelectionProcessId,
            [FromQuery] EligiblePapersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _paperService.GetFullTextEligiblePapersAsync(studySelectionProcessId, request, cancellationToken);
            return Ok(result, "Full-Text eligible papers retrieved successfully.");
        }

        /// <summary>
        /// Evaluate a paper using AI (Title/Abstract phase only)
        /// </summary>
        [HttpPost("study-selection/{studySelectionId}/papers/{paperId}/ai-evaluate")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<StuSeAIOutput>>> EvaluatePaperWithAi(
            [FromRoute] Guid studySelectionId,
            [FromRoute] Guid paperId,
            CancellationToken cancellationToken)
        {
            var userIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdStr, out var reviewerId))
            {
                throw new ArgumentException("Invalid user ID in current context.");
            }

            var result = await _stuSeAIService.EvaluateTitleAbstractAsync(studySelectionId, paperId, reviewerId, cancellationToken);
            return Ok(result, "AI evaluation completed successfully.");
        }

        /// <summary>
        /// Evaluate a paper using AI (Full-Text phase)
        /// </summary>
        [HttpPost("study-selection/{studySelectionId}/papers/{paperId}/full-text/ai-evaluate")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> EvaluateFullTextWithAi(
            [FromRoute] Guid studySelectionId,
            [FromRoute] Guid paperId,
            CancellationToken cancellationToken)
        {
            var userIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdStr, out var reviewerId))
            {
                throw new ArgumentException("Invalid user ID in current context.");
            }

            var isExist = _fullTextAiEvaluationQueue.IsProcessing(studySelectionId, paperId);
            if (isExist)
            {
                throw new InvalidOperationException("AI evaluation is already in progress for this paper. Please wait for the background job to complete.");
            }

            var enqueued = _fullTextAiEvaluationQueue.Enqueue(new StuSeFullTextAiEvaluationTask(studySelectionId, paperId, reviewerId));
            if (!enqueued)
            {
                return Ok("Failed to enqueue AI evaluation. The queue might be full.", "Queue full.");
            }

            return Ok("AI evaluation has been started in the background.", "Background job started.");
        }

        /// <summary>
        /// Get AI evaluation result for a paper (specific to the current reviewer)
        /// </summary>
        [HttpGet("study-selection/{studySelectionId}/papers/{paperId}/ai-result")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<StudySelectionAIResultResponse>>> GetAiEvaluationResult(
            [FromRoute] Guid studySelectionId,
            [FromRoute] Guid paperId,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var userIdStr = _currentUserService.GetUserId();
            if (!Guid.TryParse(userIdStr, out var reviewerId))
            {
                throw new ArgumentException("Invalid user ID in current context.");
            }

            var result = await _studySelectionAIResultService.GetByKeysAsync(studySelectionId, paperId, reviewerId, phase, cancellationToken);
            return Ok(result, "AI evaluation result retrieved successfully.");
        }

        // ============================================
        // Exclusion Reasons Management
        // ============================================

        /// <summary>
        /// Get Exclusion Reasons of a Study Selection Process
        /// </summary>
        [HttpGet("study-selection/{id}/exclusion-reasons")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StuSeExclusionCodeResponse>>>> GetExclusionReasons(
            [FromRoute] Guid id,
            [FromQuery] bool onlyActive = false,
            [FromQuery] ExclusionReasonSourceFilter source = ExclusionReasonSourceFilter.All,
            [FromQuery] string? search = null)
        {
            var result = await _exclusionCodeService.GetByProcessIdAsync(id, onlyActive, source, search);
            return Ok(result, "Exclusion reasons retrieved successfully.");
        }

        /// <summary>
        /// Add multiple Exclusion Reasons (Library and/or Custom)
        /// </summary>
        [HttpPost("study-selection/{id}/exclusion-reasons")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StuSeExclusionCodeResponse>>>> AddExclusionReasons(
            [FromRoute] Guid id,
            [FromBody] AddExclusionReasonsRequest request)
        {
            var result = await _exclusionCodeService.AddBatchAsync(id, request);
            return Created(result, "Exclusion reasons added successfully.");
        }

        /// <summary>
        /// Update an Exclusion Reason
        /// </summary>
        [HttpPut("study-selection/exclusion-reasons/{id}")]
        public async Task<ActionResult<ApiResponse<StuSeExclusionCodeResponse>>> UpdateExclusionReason(
            [FromRoute] Guid id,
            [FromBody] UpdateExclusionReasonRequest request)
        {
            var result = await _exclusionCodeService.UpdateAsync(id, request);
            return Ok(result, "Exclusion reason updated successfully.");
        }

        /// <summary>
        /// Toggle the Active state of an Exclusion Reason
        /// </summary>
        [HttpPatch("study-selection/exclusion-reasons/{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<StuSeExclusionCodeResponse>>> ToggleExclusionReasonActive(
            [FromRoute] Guid id)
        {
            var result = await _exclusionCodeService.ToggleActiveAsync(id);
            return Ok(result, $"Exclusion reason {(result.IsActive ? "activated" : "deactivated")} successfully.");
        }

        /// <summary>
        /// Delete an Exclusion Reason
        /// </summary>
        [HttpDelete("study-selection/exclusion-reasons/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteExclusionReason(
            [FromRoute] Guid id)
        {
            await _exclusionCodeService.DeleteAsync(id);
            return Ok("Exclusion reason deleted successfully.");
        }

        /// <summary>
        /// Get final resolution paper progress data for the Decision Matrix UI
        /// </summary>
        [HttpGet("study-selection/{id}/final-resolution-progress")]
        public async Task<ActionResult<ApiResponse<FinalResolutionProgressResponse>>> GetFinalResolutionPaperProgress(
            [FromRoute] Guid id,
            [FromQuery] FinalResolutionProgressRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetFinalResolutionPaperProgressAsync(id, request, cancellationToken);
            return Ok(result, "Final resolution paper progress retrieved successfully.");
        }
    }
}
