using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.AiSetupService;
using SRSS.IAM.Services.DTOs.AiSetup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/ai-setup")]
    public class AiSetupController : BaseController
    {
        private readonly IAiSetupService _aiSetupService;

        public AiSetupController(IAiSetupService aiSetupService)
        {
            _aiSetupService = aiSetupService;
        }

        [HttpPost("analyze-topic")]
        public async Task<ActionResult<ApiResponse<AnalyzeTopicResponse>>> AnalyzeTopic(
            [FromRoute] Guid projectId,
            [FromBody] AnalyzeTopicRequest request)
        {
            var result = await _aiSetupService.AnalyzeTopicAsync(request);
            return Ok(result, "Topic analyzed successfully.");
        }

        [HttpPost("generate-picoc")]
        public async Task<ActionResult<ApiResponse<GeneratePicocResponse>>> GeneratePicoc(
            [FromRoute] Guid projectId,
            [FromBody] GeneratePicocRequest request)
        {
            var result = await _aiSetupService.GeneratePicocAsync(request);
            return Ok(result, "PICO-C generated successfully.");
        }

        [HttpPost("generate-rqs")]
        public async Task<ActionResult<ApiResponse<GenerateRqsResponse>>> GenerateRqs(
            [FromRoute] Guid projectId,
            [FromBody] GenerateRqsRequest request)
        {
            var result = await _aiSetupService.GenerateRqsAsync(request);
            return Ok(result, "Research questions suggested successfully.");
        }

        [HttpGet("setup-details")]
        public async Task<ActionResult<ApiResponse<ProjectSetupDetailsResponse>>> GetSetupDetails([FromRoute] Guid projectId)
        {
            var result = await _aiSetupService.GetSetupDetailsAsync(projectId);
            return Ok(result, "Project setup details retrieved successfully.");
        }

        [HttpPut("setup-details")]
        public async Task<ActionResult<ApiResponse>> UpdateSetup(
            [FromRoute] Guid projectId,
            [FromBody] UpdateSetupRequest request)
        {
            await _aiSetupService.UpdateSetupAsync(projectId, request);
            return Ok("Project setup updated successfully.");
        }
    }
}
