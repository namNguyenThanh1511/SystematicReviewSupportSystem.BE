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