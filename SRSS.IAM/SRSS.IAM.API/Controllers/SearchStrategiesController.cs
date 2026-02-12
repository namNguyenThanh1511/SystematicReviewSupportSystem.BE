using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.SearchStrategy;
using SRSS.IAM.Services.SearchStrategyService;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/search-strategies")]
	public class SearchStrategiesController : BaseController
	{
		private readonly ISearchStrategyService _service;

		public SearchStrategiesController(ISearchStrategyService service)
		{
			_service = service;
		}

		/// <summary>
		/// Upsert một Search Strategy (Insert hoặc Update)
		/// </summary>
		[HttpPost("upsert")]
		public async Task<ActionResult<ApiResponse<SearchStrategyDto>>> UpsertStrategy(
			[FromBody] SearchStrategyDto dto)
		{
			var result = await _service.UpsertAsync(dto);
			return Ok(result, "Lưu strategy thành công");
		}

		/// <summary>
		/// Lấy tất cả Search Strategies theo Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}")]
		public async Task<ActionResult<ApiResponse<List<SearchStrategyDto>>>> GetAllByProtocolId(Guid protocolId)
		{
			var result = await _service.GetAllByProtocolIdAsync(protocolId);
			return Ok(result, "Lấy danh sách strategies thành công");
		}

		/// <summary>
		/// Xóa một Search Strategy
		/// </summary>
		[HttpDelete("{strategyId}")]
		public async Task<ActionResult<ApiResponse>> DeleteStrategy(Guid strategyId)
		{
			await _service.DeleteAsync(strategyId);
			return Ok("Xóa strategy thành công");
		}

		// ==================== Search Strings ====================

		/// <summary>
		/// Bulk Upsert Search Strings (Insert hoặc Update nhiều items cùng lúc)
		/// </summary>
		[HttpPost("search-strings/bulk")]
		public async Task<ActionResult<ApiResponse<List<SearchStringDto>>>> BulkUpsertSearchStrings(
			[FromBody] List<SearchStringDto> dtos)
		{
			var result = await _service.BulkUpsertSearchStringsAsync(dtos);
			return Ok(result, $"Lưu {result.Count} search strings thành công");
		}

		/// <summary>
		/// Lấy tất cả Search Strings theo Strategy ID
		/// </summary>
		[HttpGet("{strategyId}/search-strings")]
		public async Task<ActionResult<ApiResponse<List<SearchStringDto>>>> GetSearchStringsByStrategyId(Guid strategyId)
		{
			var result = await _service.GetSearchStringsByStrategyIdAsync(strategyId);
			return Ok(result, "Lấy danh sách search strings thành công");
		}

		// ==================== Search Terms ====================

		/// <summary>
		/// Bulk Upsert Search Terms
		/// </summary>
		[HttpPost("search-terms/bulk")]
		public async Task<ActionResult<ApiResponse<List<SearchTermDto>>>> BulkUpsertSearchTerms(
			[FromBody] List<SearchTermDto> dtos)
		{
			var result = await _service.BulkUpsertSearchTermsAsync(dtos);
			return Ok(result, $"Lưu {result.Count} search terms thành công");
		}

		/// <summary>
		/// Lấy tất cả Search Terms theo Search String ID
		/// </summary>
		[HttpGet("search-strings/{searchStringId}/terms")]
		public async Task<ActionResult<ApiResponse<List<SearchTermDto>>>> GetSearchTermsBySearchStringId(
			Guid searchStringId)
		{
			var result = await _service.GetSearchTermsBySearchStringIdAsync(searchStringId);
			return Ok(result, "Lấy danh sách terms thành công");
		}

		// ==================== Search String Terms (Junction) ====================

		/// <summary>
		/// Bulk Upsert Search String Terms (Junction records)
		/// </summary>
		[HttpPost("search-string-terms/bulk")]
		public async Task<ActionResult<ApiResponse>> BulkUpsertSearchStringTerms(
			[FromBody] List<SearchStringTermDto> dtos)
		{
			await _service.BulkUpsertSearchStringTermsAsync(dtos);
			return Ok($"Lưu {dtos.Count} junction records thành công");
		}

		/// <summary>
		/// Lấy junction records theo Search String ID
		/// </summary>
		[HttpGet("search-strings/{searchStringId}/junction")]
		public async Task<ActionResult<ApiResponse<List<SearchStringTermDto>>>> GetSearchStringTermsBySearchStringId(
			Guid searchStringId)
		{
			var result = await _service.GetSearchStringTermsBySearchStringIdAsync(searchStringId);
			return Ok(result, "Lấy junction records thành công");
		}

		// ==================== Search Sources ====================

		/// <summary>
		/// Bulk Upsert Search Sources
		/// </summary>
		[HttpPost("search-sources/bulk")]
		public async Task<ActionResult<ApiResponse<List<SearchSourceDto>>>> BulkUpsertSearchSources(
			[FromBody] List<SearchSourceDto> dtos)
		{
			var result = await _service.BulkUpsertSearchSourcesAsync(dtos);
			return Ok(result, $"Lưu {result.Count} search sources thành công");
		}

		/// <summary>
		/// Lấy tất cả Search Sources theo Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}/sources")]
		public async Task<ActionResult<ApiResponse<List<SearchSourceDto>>>> GetSearchSourcesByProtocolId(
			Guid protocolId)
		{
			var result = await _service.GetSearchSourcesByProtocolIdAsync(protocolId);
			return Ok(result, "Lấy danh sách sources thành công");
		}
	}
}