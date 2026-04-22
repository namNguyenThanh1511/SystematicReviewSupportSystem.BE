using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.SelectionCriteriaService;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/selection-criteria")]
	public class SelectionCriteriaController : BaseController
	{
		private readonly ISelectionCriteriaService _service;

		public SelectionCriteriaController(ISelectionCriteriaService service)
		{
			_service = service;
		}

		// ==================== Study Selection Criteria ====================

		/// <summary>
		/// Upsert một Study Selection Criteria
		/// </summary>
		[HttpPost("upsert")]
		public async Task<ActionResult<ApiResponse<StudySelectionCriteriaDto>>> UpsertCriteria(
			[FromBody] StudySelectionCriteriaDto dto)
		{
			var result = await _service.UpsertCriteriaAsync(dto);
			return Ok(result, "Lưu criteria thành công");
		}

		/// <summary>
		/// Lấy tất cả Criteria theo Project ID
		/// </summary>
		[HttpGet("project/{projectId}")]
		public async Task<ActionResult<ApiResponse<List<StudySelectionCriteriaDto>>>> GetAllByProjectId(
			Guid projectId)
		{
			var result = await _service.GetAllByProjectIdAsync(projectId);
			return Ok(result, "Lấy danh sách criteria thành công");
		}

		/// <summary>
		/// Xóa một Criteria
		/// </summary>
		[HttpDelete("{criteriaId}")]
		public async Task<ActionResult<ApiResponse>> DeleteCriteria(Guid criteriaId)
		{
			await _service.DeleteCriteriaAsync(criteriaId);
			return Ok("Xóa criteria thành công");
		}

		// ==================== Inclusion Criteria ====================

		/// <summary>
		/// Bulk Upsert Inclusion Criteria
		/// </summary>
		[HttpPost("inclusion/bulk")]
		public async Task<ActionResult<ApiResponse<List<InclusionCriterionDto>>>> BulkUpsertInclusionCriteria(
			[FromBody] List<InclusionCriterionDto> dtos)
		{
			var result = await _service.BulkUpsertInclusionCriteriaAsync(dtos);
			return Ok(result, $"Lưu {result.Count} inclusion criteria thành công");
		}

		/// <summary>
		/// Lấy Inclusion Criteria theo Criteria ID
		/// </summary>
		[HttpGet("{criteriaId}/inclusion")]
		public async Task<ActionResult<ApiResponse<List<InclusionCriterionDto>>>> GetInclusionByCriteriaId(
			Guid criteriaId)
		{
			var result = await _service.GetInclusionByCriteriaIdAsync(criteriaId);
			return Ok(result, "Lấy danh sách inclusion criteria thành công");
		}

		// ==================== Exclusion Criteria ====================

		/// <summary>
		/// Bulk Upsert Exclusion Criteria
		/// </summary>
		[HttpPost("exclusion/bulk")]
		public async Task<ActionResult<ApiResponse<List<ExclusionCriterionDto>>>> BulkUpsertExclusionCriteria(
			[FromBody] List<ExclusionCriterionDto> dtos)
		{
			var result = await _service.BulkUpsertExclusionCriteriaAsync(dtos);
			return Ok(result, $"Lưu {result.Count} exclusion criteria thành công");
		}

		/// <summary>
		/// Lấy Exclusion Criteria theo Criteria ID
		/// </summary>
		[HttpGet("{criteriaId}/exclusion")]
		public async Task<ActionResult<ApiResponse<List<ExclusionCriterionDto>>>> GetExclusionByCriteriaId(
			Guid criteriaId)
		{
			var result = await _service.GetExclusionByCriteriaIdAsync(criteriaId);
			return Ok(result, "Lấy danh sách exclusion criteria thành công");
		}
	}
}