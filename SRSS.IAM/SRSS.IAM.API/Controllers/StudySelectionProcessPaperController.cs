using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Common;
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
        public async Task<ActionResult<ApiResponse<PaginatedResponse<IncludedPaperResponse>>>> GetIncludedPapers(
            [FromRoute] Guid processId,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _studySelectionProcessPaperService.GetIncludedPapersByProcessIdAsync(processId, search, pageNumber, pageSize, cancellationToken);
            return Ok(result, $"Found {result.TotalCount} included papers.");
        }
    }
}
