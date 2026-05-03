using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.PaperStatisticsService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/project/{projectId}/papers")]
    public class PaperStatisticsController : BaseController
    {
        private readonly IPaperStatisticsService _statisticsService;

        public PaperStatisticsController(IPaperStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<PaperOverviewDto>>> GetOverview(
            [FromRoute] Guid projectId,
            [FromQuery] PaperStatisticsFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _statisticsService.GetOverviewAsync(projectId, filter, cancellationToken);
            return Ok(result, "Overview statistics retrieved successfully.");
        }

        [HttpGet("by-year")]
        public async Task<ActionResult<ApiResponse<List<YearCountDto>>>> GetByYear(
            [FromRoute] Guid projectId,
            [FromQuery] PaperStatisticsFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _statisticsService.GetPapersByYearAsync(projectId, filter, cancellationToken);
            return Ok(result, "Papers by year retrieved successfully.");
        }

        [HttpGet("publication-types")]
        public async Task<ActionResult<ApiResponse<List<CountItemDto>>>> GetPublicationTypes(
            [FromRoute] Guid projectId,
            [FromQuery] PaperStatisticsFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _statisticsService.GetPublicationTypesAsync(projectId, filter, cancellationToken);
            return Ok(result, "Publication types retrieved successfully.");
        }

        [HttpGet("top-journals")]
        public async Task<ActionResult<ApiResponse<List<CountItemDto>>>> GetTopJournals(
            [FromRoute] Guid projectId,
            [FromQuery] int top = 10,
            [FromQuery] PaperStatisticsFilter filter = null!,
            CancellationToken cancellationToken = default)
        {
            var result = await _statisticsService.GetTopJournalsAsync(projectId, top, filter, cancellationToken);
            return Ok(result, "Top journals retrieved successfully.");
        }

        [HttpGet("top-conferences")]
        public async Task<ActionResult<ApiResponse<List<CountItemDto>>>> GetTopConferences(
            [FromRoute] Guid projectId,
            [FromQuery] int top = 10,
            [FromQuery] PaperStatisticsFilter filter = null!,
            CancellationToken cancellationToken = default)
        {
            var result = await _statisticsService.GetTopConferencesAsync(projectId, top, filter, cancellationToken);
            return Ok(result, "Top conferences retrieved successfully.");
        }

        [HttpGet("top-publishers")]
        public async Task<ActionResult<ApiResponse<List<CountItemDto>>>> GetTopPublishers(
            [FromRoute] Guid projectId,
            [FromQuery] int top = 10,
            [FromQuery] PaperStatisticsFilter filter = null!,
            CancellationToken cancellationToken = default)
        {
            var result = await _statisticsService.GetTopPublishersAsync(projectId, top, filter, cancellationToken);
            return Ok(result, "Top publishers retrieved successfully.");
        }

        [HttpGet("languages")]
        public async Task<ActionResult<ApiResponse<List<CountItemDto>>>> GetLanguages(
            [FromRoute] Guid projectId,
            [FromQuery] PaperStatisticsFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _statisticsService.GetLanguagesAsync(projectId, filter, cancellationToken);
            return Ok(result, "Languages retrieved successfully.");
        }

        [HttpGet("fulltext-status")]
        public async Task<ActionResult<ApiResponse<List<StatusCountItemDto>>>> GetFulltextStatus(
            [FromRoute] Guid projectId,
            [FromQuery] PaperStatisticsFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _statisticsService.GetFulltextStatusAsync(projectId, filter, cancellationToken);
            return Ok(result, "Fulltext status retrieved successfully.");
        }

        [HttpGet("top-keywords")]
        public async Task<ActionResult<ApiResponse<List<CountItemDto>>>> GetTopKeywords(
            [FromRoute] Guid projectId,
            [FromQuery] int top = 20,
            [FromQuery] PaperStatisticsFilter filter = null!,
            CancellationToken cancellationToken = default)
        {
            var result = await _statisticsService.GetTopKeywordsAsync(projectId, top, filter, cancellationToken);
            return Ok(result, "Top keywords retrieved successfully.");
        }

        [HttpGet("data-quality")]
        public async Task<ActionResult<ApiResponse<DataQualityDto>>> GetDataQuality(
            [FromRoute] Guid projectId,
            [FromQuery] PaperStatisticsFilter filter,
            CancellationToken cancellationToken)
        {
            var result = await _statisticsService.GetDataQualityAsync(projectId, filter, cancellationToken);
            return Ok(result, "Data quality statistics retrieved successfully.");
        }
    }
}
