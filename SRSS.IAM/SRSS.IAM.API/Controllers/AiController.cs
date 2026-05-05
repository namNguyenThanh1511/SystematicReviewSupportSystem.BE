using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.AI;
using SRSS.IAM.Services.SearchStrategyService;
using System;
using System.Threading.Tasks;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : BaseController
    {
        private readonly ISearchStrategyAiService _aiService;

        public AiController(ISearchStrategyAiService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// Analyze PICOC and transform into structured keyword lists using AI.
        /// </summary>
        [HttpPost("projects/{projectId}/analyze-picoc")]
        public async Task<ActionResult<ApiResponse<PicocAnalysisResponse>>> AnalyzePicoc(
            [FromRoute] Guid projectId,
            [FromBody] PicocAnalysisRequest request)
        {
            var result = await _aiService.AnalyzePicocAsync(request);
            return Ok(result, "PICOC analysis completed successfully.");
        }
    }
}
