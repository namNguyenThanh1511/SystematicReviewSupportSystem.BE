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
	}
}
