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
		/// Lấy tất cả Synthesis Strategies theo Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}/synthesis-strategies")]
		public async Task<ActionResult<ApiResponse<List<DataSynthesisStrategyDto>>>> GetSynthesisStrategiesByProtocolId(
			Guid protocolId)
		{
			var result = await _service.GetSynthesisStrategiesByProtocolIdAsync(protocolId);
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

		// ==================== Dissemination Strategies ====================

		/// <summary>
		/// Upsert một Dissemination Strategy
		/// </summary>
		[HttpPost("dissemination/upsert")]
		public async Task<ActionResult<ApiResponse<DisseminationStrategyDto>>> UpsertDisseminationStrategy(
			[FromBody] DisseminationStrategyDto dto)
		{
			var result = await _service.UpsertDisseminationStrategyAsync(dto);
			return Ok(result, "Lưu dissemination strategy thành công");
		}

		/// <summary>
		/// Lấy tất cả Dissemination Strategies theo Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}/dissemination-strategies")]
		public async Task<ActionResult<ApiResponse<List<DisseminationStrategyDto>>>> GetDisseminationStrategiesByProtocolId(
			Guid protocolId)
		{
			var result = await _service.GetDisseminationStrategiesByProtocolIdAsync(protocolId);
			return Ok(result, "Lấy danh sách dissemination strategies thành công");
		}

		/// <summary>
		/// Xóa một Dissemination Strategy
		/// </summary>
		[HttpDelete("dissemination-strategies/{strategyId}")]
		public async Task<ActionResult<ApiResponse>> DeleteDisseminationStrategy(Guid strategyId)
		{
			await _service.DeleteDisseminationStrategyAsync(strategyId);
			return Ok("Xóa dissemination strategy thành công");
		}

		// ==================== Project Timetable ====================

		/// <summary>
		/// Bulk Upsert Project Timetable
		/// </summary>
		[HttpPost("timetable/bulk")]
		public async Task<ActionResult<ApiResponse<List<ProjectTimetableDto>>>> BulkUpsertTimetable(
			[FromBody] List<ProjectTimetableDto> dtos)
		{
			var result = await _service.BulkUpsertTimetableAsync(dtos);
			return Ok(result, $"Lưu {result.Count} timetable entries thành công");
		}

		/// <summary>
		/// Lấy Timetable theo Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}/timetable")]
		public async Task<ActionResult<ApiResponse<List<ProjectTimetableDto>>>> GetTimetableByProtocolId(
			Guid protocolId)
		{
			var result = await _service.GetTimetableByProtocolIdAsync(protocolId);
			return Ok(result, "Lấy timetable thành công");
		}

		/// <summary>
		/// Xóa một Timetable entry
		/// </summary>
		[HttpDelete("timetable/{timetableId}")]
		public async Task<ActionResult<ApiResponse>> DeleteTimetableEntry(Guid timetableId)
		{
			await _service.DeleteTimetableEntryAsync(timetableId);
			return Ok("Xóa timetable entry thành công");
		}
	}
}