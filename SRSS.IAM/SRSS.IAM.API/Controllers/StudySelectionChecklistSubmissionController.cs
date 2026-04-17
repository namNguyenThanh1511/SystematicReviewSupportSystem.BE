using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
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

        // 3.3 Get Submission
        [HttpGet("study-selection-checklist-submissions/{submissionId}")]
        public async Task<ActionResult<ApiResponse<ChecklistSubmissionDto>>> GetSubmission(
            [FromRoute] Guid submissionId,
            CancellationToken cancellationToken)
        {
            var result = await _submissionService.GetSubmissionAsync(submissionId, cancellationToken);
            return Ok(result);
        }
    }
}
