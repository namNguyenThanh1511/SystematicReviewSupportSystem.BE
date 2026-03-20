using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Builder;
using SRSS.IAM.Services.DataExtractionService;
using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/data-extraction-processes")]
	public class DataExtractionConductingController : BaseController
	{
		private readonly IDataExtractionConductingService _extractionService;

		public DataExtractionConductingController(IDataExtractionConductingService extractionService)
		{
			_extractionService = extractionService;
		}

		[HttpGet("{extractionProcessId}/dashboard")]
		public async Task<ActionResult<ApiResponse<ExtractionDashboardResponseDto>>> GetDashboard(
			[FromRoute] Guid extractionProcessId,
			[FromQuery] ExtractionDashboardFilterDto filter)
		{
			var result = await _extractionService.GetDashboardAsync(extractionProcessId, filter);
			return Ok(result, "Dashboard data retrieved successfully.");
		}

		[HttpPut("{extractionProcessId}/papers/{paperId}/assign")]
		public async Task<ActionResult<ApiResponse<object>>> AssignReviewers(
			[FromRoute] Guid extractionProcessId,
			[FromRoute] Guid paperId,
			[FromBody] AssignReviewersDto dto)
		{
			await _extractionService.AssignReviewersAsync(extractionProcessId, paperId, dto);
			return Ok<object>(new object(), "Reviewers assigned successfully.");
		}

		[HttpPost("{extractionProcessId}/start")]
		public async Task<ActionResult<ApiResponse<DataExtractionProcessResponse>>> Start([FromRoute] Guid extractionProcessId)
		{
			var result = await _extractionService.StartAsync(extractionProcessId);
			return Ok(result, "Data extraction process started successfully.");
		}

		[HttpPost("{extractionProcessId}/papers/{paperId}/submit")]
		public async Task<ActionResult<ApiResponse<object>>> SubmitExtraction(
			[FromRoute] Guid extractionProcessId,
			[FromRoute] Guid paperId,
			[FromBody] SubmitExtractionRequestDto request)
		{
			await _extractionService.SubmitExtractionAsync(extractionProcessId, paperId, request);
			return Ok<object>(new object(), "Data extraction submitted successfully.");
		}

		[HttpGet("{extractionProcessId}/papers/{paperId}/consensus")]
		public async Task<ActionResult<ApiResponse<ConsensusWorkspaceDto>>> GetConsensusWorkspace(
			[FromRoute] Guid extractionProcessId,
			[FromRoute] Guid paperId)
		{
			var result = await _extractionService.GetConsensusWorkspaceAsync(extractionProcessId, paperId);
			return Ok(result, "Consensus workspace retrieved successfully.");
		}

		[HttpPost("{extractionProcessId}/papers/{paperId}/consensus/submit")]
		public async Task<ActionResult<ApiResponse<object>>> SubmitConsensus(
			[FromRoute] Guid extractionProcessId,
			[FromRoute] Guid paperId,
			[FromBody] SubmitConsensusRequestDto request)
		{
			await _extractionService.SubmitConsensusAsync(extractionProcessId, paperId, request);
			return Ok<object>(new object(), "Consensus data submitted successfully.");
		}
		[HttpGet("{extractionProcessId}/export")]
		public async Task<IActionResult> ExportExtractedData([FromRoute] Guid extractionProcessId)
		{
			var fileContent = await _extractionService.ExportExtractedDataAsync(extractionProcessId);
			return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"extraction_data_{extractionProcessId}.xlsx");
		}

		[HttpPost("{extractionProcessId}/papers/{paperId}/auto-extract")]
		public async Task<ActionResult<ApiResponse<List<ExtractedValueDto>>>> AutoExtractWithAi(
			[FromRoute] Guid extractionProcessId,
			[FromRoute] Guid paperId)
		{
			var result = await _extractionService.AutoExtractWithAiAsync(extractionProcessId, paperId);
			return Ok(result, "Auto-extracted data retrieved successfully.");
		}
	}
}
