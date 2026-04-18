using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.StudySelectionChecklist;
using SRSS.IAM.Services.StudySelectionChecklists;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class StudySelectionChecklistSubmissionController : BaseController
    {
        private readonly IStudySelectionChecklistSubmissionService _submissionService;

        public StudySelectionChecklistSubmissionController(IStudySelectionChecklistSubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        // 3.1 Create Submission
        [HttpPost("study-selection-checklist-submissions")]
        public async Task<ActionResult<ApiResponse<ChecklistSubmissionDto>>> CreateSubmission(
            [FromBody] CreateSubmissionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _submissionService.CreateSubmissionAsync(request, cancellationToken);
            return Ok(result, "Submission created successfully.");
        }

        [HttpGet("study-selection-checklist-submissions/{submissionId}")]
        public async Task<ActionResult<ApiResponse<ChecklistSubmissionDto>>> GetSubmission(
            [FromRoute] Guid submissionId,
            CancellationToken cancellationToken)
        {
            var result = await _submissionService.GetSubmissionAsync(submissionId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("study-selection-checklist-submissions/context")]
        public async Task<ActionResult<ApiResponse<ChecklistReviewDto>>> GetSubmissionByContext(
            [FromQuery] Guid processId,
            [FromQuery] Guid paperId,
            [FromQuery] Guid reviewerId,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var result = await _submissionService.GetChecklistForReviewByContextAsync(processId, paperId, reviewerId, phase, cancellationToken);
            return Ok(result);
        }

        // 3.2 Get Reviewer Submission for a Paper
        [HttpGet("study-selection-checklist-submissions/reviewer-submission")]
        public async Task<ActionResult<ApiResponse<ChecklistReviewDto>>> GetReviewerSubmission(
            [FromQuery] Guid processId,
            [FromQuery] Guid paperId,
            [FromQuery] Guid reviewerId,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var result = await _submissionService.GetSubmissionByContextAsync(processId, paperId, reviewerId, phase, cancellationToken);
            return Ok(result);
        }
    }
}
