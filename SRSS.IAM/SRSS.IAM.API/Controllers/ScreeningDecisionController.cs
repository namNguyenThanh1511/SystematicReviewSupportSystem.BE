using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.StudySelectionService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/screening-decisions")]
    public class ScreeningDecisionController : BaseController
    {
        private readonly IStudySelectionService _studySelectionService;

        public ScreeningDecisionController(IStudySelectionService studySelectionService)
        {
            _studySelectionService = studySelectionService;
        }

        // 4.1 Update Screening Decision
        // PUT /screening-decisions/{decisionId}
        [HttpPut("{decisionId}")]
        public async Task<ActionResult<ApiResponse<ScreeningDecisionResponse>>> Update(
            [FromRoute] Guid decisionId,
            [FromBody] UpdateScreeningDecisionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionService.UpdateScreeningDecisionAsync(decisionId, request, cancellationToken);
            return Ok(result, "Screening decision updated successfully.");
        }
    }
}
