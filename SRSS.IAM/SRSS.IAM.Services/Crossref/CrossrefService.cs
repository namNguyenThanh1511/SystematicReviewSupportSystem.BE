using System;
using System.Collections.Generic;
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
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.Utils;

namespace SRSS.IAM.Services.Crossref;

public class CrossrefService : ICrossrefService
{
    private readonly HttpClient _httpClient;
    private readonly CrossrefSettings _settings;
    private readonly ILogger<CrossrefService> _logger;
    private readonly IRedisCacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    private const string CacheKeyPrefix = "crossref:work:";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

    public CrossrefService(
        HttpClient httpClient,
        IOptions<CrossrefSettings> options,
        ILogger<CrossrefService> logger,
        IRedisCacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    // ─── GET /works ───────────────────────────────────────────────────────────

    public async Task<CrossrefMessageList<CrossrefWorkDto>> GetWorksAsync(
        CrossrefQueryParameters parameters,
        CancellationToken ct = default)
    {
        var url = BuildWorksUrl(parameters);
        _logger.LogInformation("Fetching works from Crossref: {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            await EnsureSuccessAsync(response);

            var result = await response.Content
                .ReadFromJsonAsync<CrossrefResponse<CrossrefMessageList<CrossrefWorkDto>>>(cancellationToken: ct);

            var messageList = result?.Message
                ?? throw new InvalidOperationException("Failed to deserialize Crossref works list.");

            // Mark already imported works if ProjectId is provided
            if (parameters.ProjectId.HasValue && messageList.Items.Any())
            {
                var dois = messageList.Items
                    .Where(w => !string.IsNullOrEmpty(w.Doi))
                    .Select(w => DoiHelper.Normalize(w.Doi))
                    .Where(d => d != null)
                    .Cast<string>()
                    .ToList();

                if (dois.Any())
                {
                    var existingDois = await _unitOfWork.Papers.GetExistingDoisByProjectAsync(dois, parameters.ProjectId.Value, ct);
                    var existingSet = existingDois
                        .Select(d => DoiHelper.Normalize(d))
                        .Where(d => d != null)
                        .ToHashSet();

                    foreach (var work in messageList.Items)
                    {
                        var normalizedDoi = DoiHelper.Normalize(work.Doi);
                        if (normalizedDoi != null && existingSet.Contains(normalizedDoi))
                        {
                            work.IsImported = true;
                        }
                    }
                }
            }

            return messageList;
        }
        catch (Exception ex) when (ex is not BaseDomainException)
        {
            _logger.LogError(ex, "Error fetching works from Crossref with parameters: {@Parameters}", parameters);
            throw;
        }
    }

    // ─── GET /works/{doi} ────────────────────────────────────────────────────

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

        // Append mailto to polite pool if configured
        if (!string.IsNullOrWhiteSpace(_settings.Mailto))
            url = QueryHelpers.AddQueryString(url, "mailto", _settings.Mailto);

        _logger.LogInformation("Fetching work detail from Crossref for DOI: {Doi}", doi);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            await EnsureSuccessAsync(response);

            var result = await response.Content
                .ReadFromJsonAsync<CrossrefResponse<CrossrefWorkDto>>(cancellationToken: ct);

            var work = result?.Message
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize Crossref work detail for DOI: {doi}");

            await _cacheService.SetAsync(cacheKey, JsonSerializer.Serialize(work), CacheExpiry);

            return work;
        }
        catch (Exception ex) when (ex is not BaseDomainException)
        {
            _logger.LogError(ex, "Error fetching work detail from Crossref for DOI: {Doi}", doi);
            throw;
        }
    }

    // ─── GET /works/{doi}/agency ─────────────────────────────────────────────

    public async Task<CrossrefAgencyDto> GetAgencyByDoiAsync(string doi, CancellationToken ct = default)
    {
        var url = $"works/{WebUtility.UrlEncode(doi)}/agency";
        _logger.LogInformation("Fetching agency info from Crossref for DOI: {Doi}", doi);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            await EnsureSuccessAsync(response);

            var result = await response.Content
                .ReadFromJsonAsync<CrossrefResponse<CrossrefAgencyDto>>(cancellationToken: ct);

            return result?.Message
                ?? throw new InvalidOperationException(
                    $"Failed to deserialize Crossref agency info for DOI: {doi}");
        }
        catch (Exception ex) when (ex is not BaseDomainException)
        {
            _logger.LogError(ex, "Error fetching agency info from Crossref for DOI: {Doi}", doi);
            throw;
        }
    }

    // ─── URL builder ─────────────────────────────────────────────────────────

    private string BuildWorksUrl(CrossrefQueryParameters p)
    {
        var q = new Dictionary<string, string?>();

        // General query
        if (!string.IsNullOrEmpty(p.Query)) q["query"] = p.Query;

        // Field queries
        if (!string.IsNullOrEmpty(p.QueryAuthor)) q["query.author"] = p.QueryAuthor;
        if (!string.IsNullOrEmpty(p.QueryTitle)) q["query.title"] = p.QueryTitle;
        if (!string.IsNullOrEmpty(p.QueryBibliographic)) q["query.bibliographic"] = p.QueryBibliographic;
        if (!string.IsNullOrEmpty(p.QueryAffiliation)) q["query.affiliation"] = p.QueryAffiliation;
        if (!string.IsNullOrEmpty(p.QueryEditor)) q["query.editor"] = p.QueryEditor;
        if (!string.IsNullOrEmpty(p.QueryContributor)) q["query.contributor"] = p.QueryContributor;
        if (!string.IsNullOrEmpty(p.QueryContainerTitle)) q["query.container-title"] = p.QueryContainerTitle;
        if (!string.IsNullOrEmpty(p.QueryEventName)) q["query.event-name"] = p.QueryEventName;
        if (!string.IsNullOrEmpty(p.QueryEventLocation)) q["query.event-location"] = p.QueryEventLocation;
        if (!string.IsNullOrEmpty(p.QueryEventSponsor)) q["query.event-sponsor"] = p.QueryEventSponsor;
        if (!string.IsNullOrEmpty(p.QueryPublisherName)) q["query.publisher-name"] = p.QueryPublisherName;
        if (!string.IsNullOrEmpty(p.QueryPublisherLocation)) q["query.publisher-location"] = p.QueryPublisherLocation;
        if (!string.IsNullOrEmpty(p.QueryFunderName)) q["query.funder-name"] = p.QueryFunderName;

        // Pagination
        if (p.Rows.HasValue) q["rows"] = p.Rows.Value.ToString();
        // if (p.Offset.HasValue)                                  q["offset"]                     = p.Offset.Value.ToString();
        if (!string.IsNullOrEmpty(p.Cursor)) q["cursor"] = p.Cursor;

        // Sorting
        if (!string.IsNullOrEmpty(p.Sort)) q["sort"] = p.Sort;
        if (!string.IsNullOrEmpty(p.Order)) q["order"] = p.Order;

        // Filter / select / facet / sample
        if (!string.IsNullOrEmpty(p.Filter)) q["filter"] = p.Filter;
        if (!string.IsNullOrEmpty(p.Select)) q["select"] = p.Select;
        if (!string.IsNullOrEmpty(p.Facet)) q["facet"] = p.Facet;
        if (p.Sample.HasValue) q["sample"] = p.Sample.Value.ToString();

        // Polite pool — use param value first, fall back to global config
        var mailto = !string.IsNullOrEmpty(p.Mailto) ? p.Mailto : _settings.Mailto;
        if (!string.IsNullOrEmpty(mailto)) q["mailto"] = mailto;

        return QueryHelpers.AddQueryString("works", q);
    }

    // ─── Error handling ───────────────────────────────────────────────────────

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
