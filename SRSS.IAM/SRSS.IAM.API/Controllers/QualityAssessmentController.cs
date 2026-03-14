using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.QualityAssessmentService;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/quality-assessment")]
	public class QualityAssessmentController : BaseController
	{
		private readonly IQualityAssessmentService _service;

		public QualityAssessmentController(IQualityAssessmentService service)
		{
			_service = service;
		}

		// ==================== Quality Assessment Strategies ====================

		/// <summary>
		/// Upsert một Quality Assessment Strategy
		/// </summary>
		[HttpPost("strategies/upsert")]
		public async Task<ActionResult<ApiResponse<QualityAssessmentStrategyDto>>> UpsertStrategy(
			[FromBody] QualityAssessmentStrategyDto dto)
		{
			var result = await _service.UpsertStrategyAsync(dto);
			return Ok(result, "Lưu quality strategy thành công");
		}

		/// <summary>
		/// Lấy tất cả Quality Strategies theo Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}/strategies")]
		public async Task<ActionResult<ApiResponse<List<QualityAssessmentStrategyDto>>>> GetStrategiesByProtocolId(
			Guid protocolId)
		{
			var result = await _service.GetStrategiesByProtocolIdAsync(protocolId);
			return Ok(result, "Lấy danh sách strategies thành công");
		}

		/// <summary>
		/// Xóa một Quality Strategy
		/// </summary>
		[HttpDelete("strategies/{strategyId}")]
		public async Task<ActionResult<ApiResponse>> DeleteStrategy(Guid strategyId)
		{
			await _service.DeleteStrategyAsync(strategyId);
			return Ok("Xóa strategy thành công");
		}

		// ==================== Quality Checklists ====================

		/// <summary>
		/// Bulk Upsert Quality Checklists
		/// </summary>
		[HttpPost("checklists/bulk")]
		public async Task<ActionResult<ApiResponse<List<QualityChecklistDto>>>> BulkUpsertChecklists(
			[FromBody] List<QualityChecklistDto> dtos)
		{
			var result = await _service.BulkUpsertChecklistsAsync(dtos);
			return Ok(result, $"Lưu {result.Count} checklists thành công");
		}

		/// <summary>
		/// Lấy Checklists theo Strategy ID
		/// </summary>
		[HttpGet("strategies/{strategyId}/checklists")]
		public async Task<ActionResult<ApiResponse<List<QualityChecklistDto>>>> GetChecklistsByStrategyId(
			Guid strategyId)
		{
			var result = await _service.GetChecklistsByStrategyIdAsync(strategyId);
			return Ok(result, "Lấy danh sách checklists thành công");
		}

		// ==================== Quality Criteria ====================

		/// <summary>
		/// Bulk Upsert Quality Criteria
		/// </summary>
		[HttpPost("criteria/bulk")]
		public async Task<ActionResult<ApiResponse<List<QualityCriterionDto>>>> BulkUpsertCriteria(
			[FromBody] List<QualityCriterionDto> dtos)
		{
			var result = await _service.BulkUpsertCriteriaAsync(dtos);
			return Ok(result, $"Lưu {result.Count} quality criteria thành công");
		}

		/// <summary>
		/// Lấy Criteria theo Checklist ID
		/// </summary>
		[HttpGet("checklists/{checklistId}/criteria")]
		public async Task<ActionResult<ApiResponse<List<QualityCriterionDto>>>> GetCriteriaByChecklistId(
			Guid checklistId)
		{
			var result = await _service.GetCriteriaByChecklistIdAsync(checklistId);
			return Ok(result, "Lấy danh sách criteria thành công");
		}
	}
}