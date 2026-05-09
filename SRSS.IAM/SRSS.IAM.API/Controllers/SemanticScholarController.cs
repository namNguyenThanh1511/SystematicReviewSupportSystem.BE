using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Cache;
using Shared.Exceptions;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.SemanticScholar;
using SRSS.IAM.Services.IdentificationService;
using SRSS.IAM.Services.SemanticScholar;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.API.Controllers;

[ApiController]
[Route("api/semantic-scholar")]
public class SemanticScholarController : BaseController
{
    private readonly ISemanticScholarService _semanticScholarService;
    private readonly IIdentificationService _identificationService;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ICurrentUserService _currentUserService;

    public SemanticScholarController(
        ISemanticScholarService semanticScholarService,
        IIdentificationService identificationService,
        IRedisCacheService redisCacheService,
        ICurrentUserService currentUserService)
    {
        _semanticScholarService = semanticScholarService;
        _identificationService = identificationService;
        _redisCacheService = redisCacheService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Search papers using Semantic Scholar API
    /// </summary>
    /// <param name="keyword">Keyword to search for</param>
    /// <param name="yearFrom">Optional starting year</param>
    /// <param name="yearTo">Optional ending year</param>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 10)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of papers from Semantic Scholar</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperSearchResultDto>>>> Search(
        [FromQuery] string keyword,
        [FromQuery] int? yearFrom,
        [FromQuery] int? yearTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentException("Keyword is required for searching.");
        }

        if (page < 1)
        {
            throw new ArgumentException("Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100.");
        }

        if (yearFrom.HasValue && yearTo.HasValue && yearFrom > yearTo)
        {
            throw new ArgumentException("YearFrom must be less than or equal to YearTo.");
        }

        var request = new SemanticScholarSearchRequest
        {
            Keyword = keyword,
            YearFrom = yearFrom,
            YearTo = yearTo,
            Page = page,
            PageSize = pageSize
        };

        var result = await _semanticScholarService.SearchPapersAsync(request, ct);

        return Ok(result, "Papers retrieved successfully from Semantic Scholar.");
    }

}
