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
    public class StudySelectionChecklistManagementController : BaseController
    {
        private readonly IStudySelectionChecklistService _checklistService;

        public StudySelectionChecklistManagementController(IStudySelectionChecklistService checklistService)
        {
            _checklistService = checklistService;
        }

        // 1.1 Create Checklist Template
        [HttpPost("projects/{projectId}/study-selection-checklist-template")]
        public async Task<ActionResult<ApiResponse<StudySelectionChecklistTemplateDto>>> CreateTemplate(
            [FromRoute] Guid projectId,
            [FromBody] CreateStudySelectionChecklistTemplateRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _checklistService.CreateTemplateAsync(projectId, request, cancellationToken);
            return Ok(result, "Checklist template created successfully.");
        }

        // 1.1.b Update Checklist Template
        [HttpPut("projects/{projectId}/study-selection-checklist-template")]
        public async Task<ActionResult<ApiResponse<StudySelectionChecklistTemplateDto>>> UpdateTemplate(
            [FromRoute] Guid projectId,
            [FromBody] UpdateStudySelectionChecklistTemplateRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _checklistService.UpdateTemplateAsync(projectId, request, cancellationToken);
            return Ok(result, "Checklist template updated successfully.");
        }

        // 1.2 Get Checklist Template
        [HttpGet("projects/{projectId}/study-selection-checklist-template")]
        public async Task<ActionResult<ApiResponse<StudySelectionChecklistTemplateDto>>> GetTemplate(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken)
        {
            var result = await _checklistService.GetTemplateByProjectIdAsync(projectId, cancellationToken);
            return Ok(result);
        }
    }
}
