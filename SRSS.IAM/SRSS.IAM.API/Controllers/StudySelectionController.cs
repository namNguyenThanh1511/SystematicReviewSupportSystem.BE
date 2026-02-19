using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.StudySelectionService;

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

        public StudySelectionController(IStudySelectionService studySelectionService)
        {
            _studySelectionService = studySelectionService;
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
        /// Get all papers with their decisions and resolutions
        /// </summary>
        [HttpGet("study-selection/{id}/papers")]
        public async Task<ActionResult<ApiResponse<List<PaperWithDecisionsResponse>>>> GetPapersWithDecisions(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.GetPapersWithDecisionsAsync(id, cancellationToken);
            return Ok(result, $"Retrieved {result.Count} papers with decisions.");
        }
    }
}
