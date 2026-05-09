using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRSS.IAM.Services.Configurations;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.SemanticScholar;
using Shared.Cache;

namespace SRSS.IAM.Services.SemanticScholar;

public class SemanticScholarApiClient : ISemanticScholarApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SemanticScholarSettings _settings;
    private readonly ILogger<SemanticScholarApiClient> _logger;
    private readonly SemanticScholarMetrics _metrics;
    private readonly IRedisCacheService _redisCache;

    public SemanticScholarApiClient(
        HttpClient httpClient,
        IHttpClientFactory httpClientFactory,
        IOptions<SemanticScholarSettings> options,
        ILogger<SemanticScholarApiClient> logger,
        SemanticScholarMetrics metrics,
        IRedisCacheService redisCache)
    {
        _httpClient = httpClient;
        _httpClientFactory = httpClientFactory;
        _settings = options.Value;
        _logger = logger;
        _metrics = metrics;
        _redisCache = redisCache;
    }

    public async Task<PaginatedResponse<PaperSearchResultDto>> ExecuteSearchAsync(SemanticScholarSearchRequest request, CancellationToken ct)
    {
        var offset = (request.Page - 1) * request.PageSize;
        var limit = request.PageSize;

        var queryParams = new List<string>
        {
            $"query={Uri.EscapeDataString(request.Keyword)}",
            $"offset={offset}",
            $"limit={limit}",
            "fields=paperId,title,authors,year,abstract,url,externalIds,journal,publicationTypes,openAccessPdf"
        };

        if (request.YearFrom.HasValue || request.YearTo.HasValue)
        {
            var yearRange = request.YearFrom.HasValue && request.YearTo.HasValue
                ? $"{request.YearFrom}-{request.YearTo}"
                : request.YearFrom.HasValue ? $"{request.YearFrom}-" : $"-{request.YearTo}";

            queryParams.Add($"year={yearRange}");
        }

        var url = $"paper/search?{string.Join("&", queryParams)}";

        try
        {
            // Attach API key header
            if (!string.IsNullOrEmpty(_settings.ApiKey))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
                    _httpClient.DefaultRequestHeaders.Remove("x-api-key");
                _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
            }

            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var response = await _httpClient.GetAsync(url, ct);
            var duration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime;

            _metrics.RecordExternalCall(duration);

            await EnsureSuccessAsync(response);

            var apiResult = await response.Content.ReadFromJsonAsync<SemanticScholarSearchApiResponse>(cancellationToken: ct);

            if (apiResult == null)
                throw new InvalidOperationException("Failed to deserialize Semantic Scholar API response.");

            var mappedData = apiResult.Data.Select(p => 
            {
                var pdfUrl = p.OpenAccessPdf?.Url ?? (string.IsNullOrEmpty(p.ExternalIds?.ArXiv) ? null : $"https://arxiv.org/pdf/{p.ExternalIds.ArXiv.Replace("arXiv:", "", StringComparison.OrdinalIgnoreCase)}.pdf");
                return new PaperSearchResultDto
                {
                    PaperId = p.PaperId,
                    Title = p.Title,
                    Authors = p.Authors?.Select(a => a.Name).ToList() ?? new List<string>(),
                    Abstract = p.Abstract,
                    Year = p.Year,
                    Doi = p.ExternalIds?.DOI,
                    Journal = p.Journal?.Name,
                    Url = p.Url,
                    OpenAccessPdfUrl = pdfUrl,
                    PdfStatus = PaperSearchResultDto.ClassifyPdfStatus(pdfUrl)
                };
            }).ToList();

            // 11. Add Logging for PDF Field Only (No Network Logging)
            var arxivCount = mappedData.Count(p => p.PdfStatus == "AVAILABLE" && p.OpenAccessPdfUrl?.Contains("arxiv.org") == true);
            _logger.LogInformation("Semantic Scholar Search: Classified {Total} papers. ArXiv count: {ArXivCount}", mappedData.Count, arxivCount);

            // 10. Sort Results by PDF Availability (AVAILABLE > LIKELY_AVAILABLE > OTHERS)
            mappedData = mappedData
                .OrderBy(p => GetPdfStatusPriority(p.PdfStatus))
                .ToList();

            return new PaginatedResponse<PaperSearchResultDto>
            {
                Items = mappedData,
                TotalCount = apiResult.Total,
                PageNumber = request.Page,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _metrics.RecordFailedCall();
            _logger.LogError(ex, "Semantic Scholar external API call failed for keyword: {Keyword}", request.Keyword);
            throw;
        }
    }

    private int GetPdfStatusPriority(string status)
    {
        return status switch
        {
            "AVAILABLE" => 1,
            "LIKELY_AVAILABLE" => 2,
            "UNKNOWN" => 3,
            "NONE" => 4,
            _ => 5
        };
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("Semantic Scholar API error: {StatusCode} - {Content}", response.StatusCode, content);

        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                throw new Shared.Exceptions.BadRequestException($"Semantic Scholar API Bad Request: {content}");
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new Shared.Exceptions.ForbiddenException("Semantic Scholar API access denied. Check API key.");
            case HttpStatusCode.NotFound:
                throw new Shared.Exceptions.NotFoundException("Semantic Scholar resource not found.");
            case HttpStatusCode.TooManyRequests:
                throw new InvalidOperationException("Semantic Scholar API rate limit exceeded.");
            default:
                throw new InvalidOperationException($"Semantic Scholar API error: {response.StatusCode}");
        }
    }
}
