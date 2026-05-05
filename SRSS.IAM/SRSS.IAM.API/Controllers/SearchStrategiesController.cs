using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.SearchStrategy;
using SRSS.IAM.Services.SearchStrategyService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api")]
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
        /// Thêm mới hoặc cập nhật Search Source đơn lẻ
        /// </summary>
        [HttpPost("search-sources")]
        public async Task<ActionResult<ApiResponse<SearchSourceDto>>> AddSearchSource(
            [FromBody] SearchSourceDto dto)
        {
            var result = await _service.AddSearchSourceAsync(dto);
            return Ok(result, "Lưu search source thành công");
        }

        /// <summary>
        /// Cập nhật danh sách Search Strategies cho một Search Source
        /// </summary>
        [HttpPut("search-sources/{sourceId}/strategies")]
        public async Task<ActionResult<ApiResponse<SearchSourceDto>>> UpdateSearchStrategies(
            Guid sourceId, [FromBody] List<SearchStrategyDto> strategies)
        {
            var result = await _service.UpdateSearchStrategiesAsync(sourceId, strategies);
            return Ok(result, "Cập nhật search strategies thành công");
        }

        /// <summary>
        /// Lấy tất cả Search Sources theo Project ID
        /// </summary>
        [HttpGet("projects/{projectId}/sources")]
        public async Task<ActionResult<ApiResponse<List<SearchSourceDto>>>> GetSearchSourcesByProjectId(
            Guid projectId)
        {
            var result = await _service.GetSearchSourcesByProjectIdAsync(projectId);
            return Ok(result, "Lấy danh sách sources thành công");
        }
    }
}