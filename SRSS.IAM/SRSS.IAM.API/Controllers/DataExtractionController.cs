using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DataExtractionService;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/data-extraction")]
	public class DataExtractionController : BaseController
	{
		private readonly IDataExtractionService _service;

		public DataExtractionController(IDataExtractionService service)
		{
			_service = service;
		}

		// ==================== Extraction Strategies ====================

		/// <summary>
		/// Upsert một Data Extraction Strategy
		/// </summary>
		[HttpPost("strategies/upsert")]
		public async Task<ActionResult<ApiResponse<DataExtractionStrategyDto>>> UpsertStrategy(
			[FromBody] DataExtractionStrategyDto dto)
		{
			var result = await _service.UpsertStrategyAsync(dto);
			return Ok(result, "Lưu extraction strategy thành công");
		}

		/// <summary>
		/// Lấy tất cả Extraction Strategies theo Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}/strategies")]
		public async Task<ActionResult<ApiResponse<List<DataExtractionStrategyDto>>>> GetStrategiesByProtocolId(
			Guid protocolId)
		{
			var result = await _service.GetStrategiesByProtocolIdAsync(protocolId);
			return Ok(result, "Lấy danh sách strategies thành công");
		}

		/// <summary>
		/// Xóa một Extraction Strategy
		/// </summary>
		[HttpDelete("strategies/{strategyId}")]
		public async Task<ActionResult<ApiResponse>> DeleteStrategy(Guid strategyId)
		{
			await _service.DeleteStrategyAsync(strategyId);
			return Ok("Xóa strategy thành công");
		}

		// ==================== Extraction Forms ====================

		/// <summary>
		/// Bulk Upsert Extraction Forms
		/// </summary>
		[HttpPost("forms/bulk")]
		public async Task<ActionResult<ApiResponse<List<DataExtractionFormDto>>>> BulkUpsertForms(
			[FromBody] List<DataExtractionFormDto> dtos)
		{
			var result = await _service.BulkUpsertFormsAsync(dtos);
			return Ok(result, $"Lưu {result.Count} forms thành công");
		}

		/// <summary>
		/// Lấy Forms theo Strategy ID
		/// </summary>
		[HttpGet("strategies/{strategyId}/forms")]
		public async Task<ActionResult<ApiResponse<List<DataExtractionFormDto>>>> GetFormsByStrategyId(
			Guid strategyId)
		{
			var result = await _service.GetFormsByStrategyIdAsync(strategyId);
			return Ok(result, "Lấy danh sách forms thành công");
		}

		// ==================== Data Items ====================

		/// <summary>
		/// Bulk Upsert Data Item Definitions
		/// </summary>
		[HttpPost("data-items/bulk")]
		public async Task<ActionResult<ApiResponse<List<DataItemDefinitionDto>>>> BulkUpsertDataItems(
			[FromBody] List<DataItemDefinitionDto> dtos)
		{
			var result = await _service.BulkUpsertDataItemsAsync(dtos);
			return Ok(result, $"Lưu {result.Count} data items thành công");
		}

		/// <summary>
		/// Lấy Data Items theo Form ID
		/// </summary>
		[HttpGet("forms/{formId}/data-items")]
		public async Task<ActionResult<ApiResponse<List<DataItemDefinitionDto>>>> GetDataItemsByFormId(
			Guid formId)
		{
			var result = await _service.GetDataItemsByFormIdAsync(formId);
			return Ok(result, "Lấy danh sách data items thành công");
		}
	}
}