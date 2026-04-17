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
    public class StudySelectionReviewerChecklistController : BaseController
    {
        private readonly IStudySelectionChecklistService _checklistService;
        private readonly IStudySelectionChecklistSubmissionService _submissionService;

        public StudySelectionReviewerChecklistController(
            IStudySelectionChecklistService checklistService,
            IStudySelectionChecklistSubmissionService submissionService)
        {
            _checklistService = checklistService;
            _submissionService = submissionService;
        }

        // 2.1 Get Checklist for Paper
        [HttpGet("study-selection/{processId}/papers/{paperId}/checklist")]
        public async Task<ActionResult<ApiResponse<PaperChecklistResponse>>> GetChecklistForPaper(
            [FromRoute] Guid processId,
            [FromRoute] Guid paperId,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var result = await _checklistService.GetChecklistForPaperAsync(processId, paperId, phase, cancellationToken);
            return Ok(result);
        }

        // 2.2 Get Existing Submission
        [HttpGet("study-selection/{processId}/papers/{paperId}/checklist-submission")]
        public async Task<ActionResult<ApiResponse<ChecklistSubmissionDto>>> GetExistingSubmission(
            [FromRoute] Guid processId,
            [FromRoute] Guid paperId,
            [FromQuery] ScreeningPhase phase,
            CancellationToken cancellationToken)
        {
            var result = await _submissionService.GetSubmissionByPaperAndPhaseAsync(paperId, phase, cancellationToken);
            if (result == null)
            {
                throw new InvalidOperationException("No checklist submission found for this paper and phase.");
            }
            return Ok(result);
        }
    }
}
