using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.SynthesisExecution;
using SRSS.IAM.Services.SynthesisExecutionService;
using System;
using System.Threading.Tasks;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing the Synthesis Execution Phase Workspace
    /// </summary>
    [ApiController]
    [Route("api/synthesis-execution")]
    public class SynthesisExecutionController : BaseController
    {
        private readonly ISynthesisExecutionService _synthesisExecutionService;

        public SynthesisExecutionController(ISynthesisExecutionService synthesisExecutionService)
        {
            _synthesisExecutionService = synthesisExecutionService;
        }

        /// <summary>
        /// Get the full workspace for synthesis (process status, themes, findings)
        /// </summary>
        [HttpGet("{reviewProcessId}/workspace")]
        public async Task<ActionResult<ApiResponse<SynthesisWorkspaceDto>>> GetWorkspace([FromRoute] Guid reviewProcessId)
        {
            var result = await _synthesisExecutionService.GetSynthesisWorkspaceAsync(reviewProcessId);
            return Ok(result, "Workspace fetched successfully.");
        }

        /// <summary>
        /// Start the synthesis process, initializing finding drafts based on RQs
        /// </summary>
        [HttpPost("{reviewProcessId}/start")]
        public async Task<ActionResult<ApiResponse<SynthesisProcessDto>>> StartProcess([FromRoute] Guid reviewProcessId)
        {
            var result = await _synthesisExecutionService.StartSynthesisProcessAsync(reviewProcessId);
            return Ok(result, "Synthesis process started successfully.");
        }

        /// <summary>
        /// Complete the semantic synthesis process
        /// </summary>
        [HttpPost("{reviewProcessId}/complete")]
        public async Task<ActionResult<ApiResponse>> CompleteProcess([FromRoute] Guid reviewProcessId)
        {
            await _synthesisExecutionService.CompleteSynthesisProcessAsync(reviewProcessId);
            return Ok("Synthesis process completed successfully.");
        }

        /// <summary>
        /// Fetch extracted raw data ready for coding operations
        /// </summary>
        [HttpGet("{reviewProcessId}/extracted-data")]
        public async Task<ActionResult<ApiResponse<System.Collections.Generic.List<SourceDataGroupDto>>>> GetExtractedData([FromRoute] Guid reviewProcessId)
        {
            var result = await _synthesisExecutionService.GetExtractedDataForSynthesisAsync(reviewProcessId);
            return Ok(result, "Extracted data loaded successfully.");
        }

        /// <summary>
        /// Create a new thematic code
        /// </summary>
        [HttpPost("{processId}/themes")]
        public async Task<ActionResult<ApiResponse<SynthesisThemeDto>>> CreateTheme([FromRoute] Guid processId, [FromBody] CreateThemeRequest request)
        {
            var result = await _synthesisExecutionService.CreateThemeAsync(processId, request);
            return Ok(result, "Theme created successfully.");
        }

        /// <summary>
        /// Update an existing thematic code
        /// </summary>
        [HttpPut("themes/{themeId}")]
        public async Task<ActionResult<ApiResponse>> UpdateTheme([FromRoute] Guid themeId, [FromBody] UpdateThemeRequest request)
        {
            await _synthesisExecutionService.UpdateThemeAsync(themeId, request);
            return Ok("Theme updated successfully.");
        }

        /// <summary>
        /// Delete a thematic code
        /// </summary>
        [HttpDelete("themes/{themeId}")]
        public async Task<ActionResult<ApiResponse>> DeleteTheme([FromRoute] Guid themeId)
        {
            await _synthesisExecutionService.DeleteThemeAsync(themeId);
            return Ok("Theme deleted successfully.");
        }

        /// <summary>
        /// Link an extracted data value to a thematic code
        /// </summary>
        [HttpPost("themes/{themeId}/evidence")]
        public async Task<ActionResult<ApiResponse<ThemeEvidenceDto>>> AddEvidence([FromRoute] Guid themeId, [FromBody] AddEvidenceRequest request)
        {
            var result = await _synthesisExecutionService.AddEvidenceToThemeAsync(themeId, request);
            return Ok(result, "Evidence linked securely.");
        }

        /// <summary>
        /// Unlink an extracted data value from a theme
        /// </summary>
        [HttpDelete("evidence/{evidenceId}")]
        public async Task<ActionResult<ApiResponse>> RemoveEvidence([FromRoute] Guid evidenceId)
        {
            await _synthesisExecutionService.RemoveEvidenceAsync(evidenceId);
            return Ok("Evidence unlinked securely.");
        }

        /// <summary>
        /// Save the drafted narrative answer for a research question
        /// </summary>
        [HttpPut("findings/{findingId}")]
        public async Task<ActionResult<ApiResponse>> SaveFinding([FromRoute] Guid findingId, [FromBody] SaveFindingRequest request)
        {
            await _synthesisExecutionService.SaveFindingAsync(findingId, request);
            return Ok("Finding updated successfully.");
        }
    }
}
