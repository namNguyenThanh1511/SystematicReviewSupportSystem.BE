using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Cache;
using Shared.Exceptions;
using SRSS.IAM.Services.Configurations;
using SRSS.IAM.Services.DTOs.Crossref;

namespace SRSS.IAM.Services.Crossref;

public class CrossrefService : ICrossrefService
{
    private readonly HttpClient _httpClient;
    private readonly CrossrefSettings _settings;
    private readonly ILogger<CrossrefService> _logger;
    private readonly IRedisCacheService _cacheService;

    private const string CacheKeyPrefix = "crossref:work:";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

    public CrossrefService(
        HttpClient httpClient,
        IOptions<CrossrefSettings> options,
        ILogger<CrossrefService> logger,
        IRedisCacheService cacheService)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<CrossrefMessageList<CrossrefWorkDto>> GetWorksAsync(CrossrefQueryParameters parameters, CancellationToken ct = default)
    {
        var url = BuildWorksUrl(parameters);
        _logger.LogInformation("Fetching works from Crossref: {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            await EnsureSuccessAsync(response);

            var result = await response.Content.ReadFromJsonAsync<CrossrefResponse<CrossrefMessageList<CrossrefWorkDto>>>(cancellationToken: ct);
            return result?.Message ?? throw new InvalidOperationException("Failed to deserialize Crossref works list.");
        }
        catch (Exception ex) when (ex is not BaseDomainException)
        {
            _logger.LogError(ex, "Error fetching works from Crossref with parameters: {@Parameters}", parameters);
            throw;
        }
    }

    public async Task<CrossrefWorkDto> GetWorkByDoiAsync(string doi, CancellationToken ct = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{doi}";
        var cachedWork = await _cacheService.GetAsync<CrossrefWorkDto>(cacheKey);
        if (cachedWork != null)
        {
            _logger.LogInformation("Cache hit for Crossref work DOI: {Doi}", doi);
            return cachedWork;
        }

        var url = $"works/{WebUtility.UrlEncode(doi)}";
        _logger.LogInformation("Fetching work detail from Crossref for DOI: {Doi}", doi);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            await EnsureSuccessAsync(response);

            var result = await response.Content.ReadFromJsonAsync<CrossrefResponse<CrossrefWorkDto>>(cancellationToken: ct);
            var work = result?.Message ?? throw new InvalidOperationException($"Failed to deserialize Crossref work detail for DOI: {doi}");

            await _cacheService.SetAsync(cacheKey, JsonSerializer.Serialize(work), CacheExpiry);
            
            return work;
        }
        catch (Exception ex) when (ex is not BaseDomainException)
        {
            _logger.LogError(ex, "Error fetching work detail from Crossref for DOI: {Doi}", doi);
            throw;
        }
    }

    public async Task<CrossrefAgencyDto> GetAgencyByDoiAsync(string doi, CancellationToken ct = default)
    {
        var url = $"works/{WebUtility.UrlEncode(doi)}/agency";
        _logger.LogInformation("Fetching agency info from Crossref for DOI: {Doi}", doi);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            await EnsureSuccessAsync(response);

            var result = await response.Content.ReadFromJsonAsync<CrossrefResponse<CrossrefAgencyDto>>(cancellationToken: ct);
            return result?.Message ?? throw new InvalidOperationException($"Failed to deserialize Crossref agency info for DOI: {doi}");
        }
        catch (Exception ex) when (ex is not BaseDomainException)
        {
            _logger.LogError(ex, "Error fetching agency info from Crossref for DOI: {Doi}", doi);
            throw;
        }
    }

    private string BuildWorksUrl(CrossrefQueryParameters parameters)
    {
        var queryParams = new Dictionary<string, string?>();

        if (!string.IsNullOrEmpty(parameters.Query)) queryParams.Add("query", parameters.Query);
        if (!string.IsNullOrEmpty(parameters.QueryAuthor)) queryParams.Add("query.author", parameters.QueryAuthor);
        if (!string.IsNullOrEmpty(parameters.QueryTitle)) queryParams.Add("query.title", parameters.QueryTitle);
        if (!string.IsNullOrEmpty(parameters.QueryBibliographic)) queryParams.Add("query.bibliographic", parameters.QueryBibliographic);
        if (!string.IsNullOrEmpty(parameters.Filter)) queryParams.Add("filter", parameters.Filter);
        if (!string.IsNullOrEmpty(parameters.Sort)) queryParams.Add("sort", parameters.Sort);
        if (!string.IsNullOrEmpty(parameters.Order)) queryParams.Add("order", parameters.Order);
        if (!string.IsNullOrEmpty(parameters.Facet)) queryParams.Add("facet", parameters.Facet);
        if (!string.IsNullOrEmpty(parameters.Select)) queryParams.Add("select", parameters.Select);
        if (parameters.Rows.HasValue) queryParams.Add("rows", parameters.Rows.Value.ToString());
        if (parameters.Offset.HasValue) queryParams.Add("offset", parameters.Offset.Value.ToString());
        if (!string.IsNullOrEmpty(parameters.Cursor)) queryParams.Add("cursor", parameters.Cursor);
        if (parameters.Sample.HasValue) queryParams.Add("sample", parameters.Sample.Value.ToString());

        return QueryHelpers.AddQueryString("works", queryParams);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("Crossref API error: {StatusCode} - {Content}", response.StatusCode, content);

        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                throw new BadRequestException($"Crossref API Bad Request: {content}");
            case HttpStatusCode.Forbidden:
                throw new ForbiddenException("Crossref API access forbidden. Check usage policies or User-Agent.");
            case HttpStatusCode.NotFound:
                throw new NotFoundException("Crossref resource not found.");
            case HttpStatusCode.TooManyRequests:
                throw new InvalidOperationException("Crossref API rate limit exceeded. Please try again later.");
            default:
                throw new InvalidOperationException($"Crossref API error: {response.StatusCode} - {content}");
        }
    }
}
