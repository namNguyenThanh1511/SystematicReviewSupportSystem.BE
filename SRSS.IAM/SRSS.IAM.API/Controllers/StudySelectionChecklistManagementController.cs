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



        // 1.2 Get All Checklist Templates for Project
        [HttpGet("projects/{projectId}/study-selection-checklist-template")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudySelectionChecklistTemplateSummaryDto>>>> GetTemplates(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken)
        {
            var result = await _checklistService.GetTemplatesByProjectIdAsync(projectId, cancellationToken);
            return Ok(result);
        }

        // 1.2.1 Get Checklist Template Detail
        [HttpGet("projects/{projectId}/study-selection-checklist-templates/{templateId}")]
        public async Task<ActionResult<ApiResponse<StudySelectionChecklistTemplateDto>>> GetTemplateDetail(
            [FromRoute] Guid projectId,
            [FromRoute] Guid templateId,
            CancellationToken cancellationToken)
        {
            var result = await _checklistService.GetTemplateDetailAsync(projectId, templateId, cancellationToken);
            return Ok(result);
        }

        // 1.3 Activate Checklist Template
        [HttpPost("study-selection-checklist-templates/{templateId}/activate")]
        public async Task<ActionResult<ApiResponse<bool>>> ActivateTemplate(
            [FromRoute] Guid templateId,
            CancellationToken cancellationToken)
        {
            var result = await _checklistService.ActivateTemplateAsync(templateId, cancellationToken);
            return Ok(result, "Checklist template activated successfully.");
        }
    }
}
