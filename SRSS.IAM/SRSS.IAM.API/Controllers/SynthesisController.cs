using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Synthesis;
using SRSS.IAM.Services.SynthesisService;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/synthesis")]
	public class SynthesisController : BaseController
	{
		private readonly ISynthesisService _service;

		public SynthesisController(ISynthesisService service)
		{
			_service = service;
		}

		// ==================== Data Synthesis Strategies ====================

		/// <summary>
		/// Upsert một Data Synthesis Strategy
		/// </summary>
		[HttpPost("strategies/upsert")]
		public async Task<ActionResult<ApiResponse<DataSynthesisStrategyDto>>> UpsertSynthesisStrategy(
			[FromBody] DataSynthesisStrategyDto dto)
		{
			var result = await _service.UpsertSynthesisStrategyAsync(dto);
			return Ok(result, "Lưu synthesis strategy thành công");
		}

		/// <summary>
		/// Lấy tất cả Synthesis Strategies theo Project ID
		/// </summary>
		[HttpGet("project/{projectId}/synthesis-strategies")]
		public async Task<ActionResult<ApiResponse<List<DataSynthesisStrategyDto>>>> GetSynthesisStrategiesByProjectId(
			Guid projectId)
		{
			var result = await _service.GetSynthesisStrategiesByProjectIdAsync(projectId);
			return Ok(result, "Lấy danh sách synthesis strategies thành công");
		}

		/// <summary>
		/// Xóa một Synthesis Strategy
		/// </summary>
		[HttpDelete("synthesis-strategies/{strategyId}")]
		public async Task<ActionResult<ApiResponse>> DeleteSynthesisStrategy(Guid strategyId)
		{
			await _service.DeleteSynthesisStrategyAsync(strategyId);
			return Ok("Xóa synthesis strategy thành công");
		}




	}
}