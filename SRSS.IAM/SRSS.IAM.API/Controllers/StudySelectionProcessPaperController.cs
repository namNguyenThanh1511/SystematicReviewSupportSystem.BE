using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.StudySelectionProcessPaperService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/study-selection")]
    public class StudySelectionProcessPaperController : BaseController
    {
        private readonly IStudySelectionProcessPaperService _studySelectionProcessPaperService;

        public StudySelectionProcessPaperController(IStudySelectionProcessPaperService studySelectionProcessPaperService)
        {
            _studySelectionProcessPaperService = studySelectionProcessPaperService;
        }

        /// <summary>
        /// Get final included papers for a Study Selection Process (Post-FullText Screening)
        /// </summary>
        [HttpGet("{processId}/included-papers")]
        public async Task<ActionResult<ApiResponse<List<IncludedPaperResponse>>>> GetIncludedPapers(
            [FromRoute] Guid processId,
            CancellationToken cancellationToken)
        {
            var result = await _studySelectionProcessPaperService.GetIncludedPapersByProcessIdAsync(processId, cancellationToken);
            return Ok(result, $"Found {result.Count} included papers.");
        }
    }
}
