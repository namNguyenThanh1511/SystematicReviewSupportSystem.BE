using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.ChecklistService;
using SRSS.IAM.Services.DTOs.Checklist;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChecklistController : BaseController
    {
        private readonly IChecklistTemplateService _templateService;
        private readonly IReviewChecklistService _reviewChecklistService;

        public ChecklistController(IChecklistTemplateService templateService, IReviewChecklistService reviewChecklistService)
        {
            _templateService = templateService;
            _reviewChecklistService = reviewChecklistService;
        }

        [HttpGet("checklist/templates")]
        public async Task<ActionResult<ApiResponse<List<ChecklistTemplateSummaryDto>>>> GetTemplates([FromQuery] bool? isSystem, CancellationToken cancellationToken)
        {
            var result = await _templateService.GetAllTemplatesAsync(isSystem, cancellationToken);
            return Ok(result, "Checklist templates retrieved successfully.");
        }

        [HttpGet("checklist/templates/{id:guid}")]
        public async Task<ActionResult<ApiResponse<ChecklistTemplateDetailDto>>> GetTemplateById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _templateService.GetTemplateByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return base.NotFound(ResponseBuilder.NotFound("Checklist template not found."));
            }

            return Ok(result, "Checklist template retrieved successfully.");
        }

        [HttpPost("checklist/templates")]
        public async Task<ActionResult<ApiResponse<ChecklistTemplateDetailDto>>> CreateTemplate([FromBody] CreateChecklistTemplateDto request, CancellationToken cancellationToken)
        {
            var result = await _templateService.CreateCustomTemplateAsync(request, cancellationToken);
            return Created(result, "Checklist template created successfully.");
        }

        [HttpPost("reviews/{reviewId:guid}/checklist")]
        public async Task<ActionResult<ApiResponse<ReviewChecklistDto>>> CloneTemplateToReview([FromRoute] Guid reviewId, [FromBody] CloneChecklistRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _templateService.CloneTemplateToReviewAsync(request.TemplateId, reviewId, cancellationToken);
            return Created(result, "Checklist created for review successfully.");
        }

        [HttpGet("reviews/{reviewId:guid}/checklist")]
        public async Task<ActionResult<ApiResponse<List<ReviewChecklistSummaryDto>>>> GetReviewChecklist([FromRoute] Guid reviewId, CancellationToken cancellationToken)
        {
            var result = await _reviewChecklistService.GetReviewChecklistsAsync(reviewId, cancellationToken);
            return Ok(result, "Review checklists retrieved successfully.");
        }

        [HttpGet("checklist/{checkListId:guid}")]
        public async Task<ActionResult<ApiResponse<ReviewChecklistDto>>> GetChecklistById([FromRoute] Guid checkListId, CancellationToken cancellationToken)
        {
            var result = await _reviewChecklistService.GetChecklistByIdAsync(checkListId, cancellationToken);
            if (result == null)
            {
                return base.NotFound(ResponseBuilder.NotFound("Review checklist not found."));
            }

            return Ok(result, "Review checklist retrieved successfully.");
        }

        [HttpPut("checklist/{checkListId:guid}/items/{itemId:guid}")]
        public async Task<ActionResult<ApiResponse<ChecklistItemResponseDto>>> UpdateChecklistItem([FromRoute] Guid checkListId, [FromRoute] Guid itemId, [FromBody] UpdateChecklistItemDto request, CancellationToken cancellationToken)
        {
            var checklist = await _reviewChecklistService.GetChecklistByIdAsync(checkListId, cancellationToken);
            if (checklist == null)
            {
                return base.NotFound(ResponseBuilder.NotFound("Review checklist not found."));
            }

            var result = await _reviewChecklistService.UpdateItemResponseAsync(checkListId, itemId, request, cancellationToken);
            return Ok(result, "Checklist item updated successfully.");
        }

        [HttpGet("checklist/{checkListId:guid}/completion")]
        public async Task<ActionResult<ApiResponse<ChecklistCompletionDto>>> GetCompletion([FromRoute] Guid checkListId, CancellationToken cancellationToken)
        {
            var checklist = await _reviewChecklistService.GetChecklistByIdAsync(checkListId, cancellationToken);
            if (checklist == null)
            {
                return base.NotFound(ResponseBuilder.NotFound("Review checklist not found."));
            }

            var result = await _reviewChecklistService.CalculateCompletionPercentageAsync(checkListId, cancellationToken);
            return Ok(result, "Checklist completion calculated successfully.");
        }

        [HttpPost("checklist/{checkListId:guid}/generate-report")]
        public async Task<IActionResult> GenerateReport([FromRoute] Guid checkListId, [FromBody] GenerateReportRequest request, CancellationToken cancellationToken)
        {
            var file = await _reviewChecklistService.GenerateReportAsync(checkListId, request, cancellationToken);
            return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"checklist-report-{checkListId}.docx");
        }
    }
}
