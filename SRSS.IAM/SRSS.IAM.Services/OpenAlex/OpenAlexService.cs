using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Exceptions;
using SRSS.IAM.Services.Configurations;
using SRSS.IAM.Services.DTOs.OpenAlex;

namespace SRSS.IAM.Services.OpenAlex
{
    public class OpenAlexService : IOpenAlexService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAlexSettings _settings;
        private readonly ILogger<OpenAlexService> _logger;

        public OpenAlexService(
            HttpClient httpClient,
            IOptions<OpenAlexSettings> options,
            ILogger<OpenAlexService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<WorkDetailDto> GetWorkAsync(string workId, CancellationToken ct)
        {
            var select = "id,display_name,doi,publication_year,type,cited_by_count,referenced_works_count,cited_by_percentile_year";
            var url = $"works/{workId}?select={select}&api_key={_settings.ApiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                await EnsureSuccessAsync(response);

                var work = await response.Content.ReadFromJsonAsync<WorkDetailDto>(cancellationToken: ct);
                return work ?? throw new InvalidOperationException("Failed to deserialize OpenAlex work detail.");
            }
            catch (Exception ex) when (ex is not BaseDomainException)
            {
                _logger.LogError(ex, "Error fetching work {WorkId} from OpenAlex", workId);
                throw;
            }
        }

        public async Task<ReferenceResultDto> GetReferencesAsync(string workId, CancellationToken ct)
        {
            var select = "referenced_works,referenced_works_count";
            var url = $"works/{workId}?select={select}&api_key={_settings.ApiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                await EnsureSuccessAsync(response);

                var result = await response.Content.ReadFromJsonAsync<ReferenceResultDto>(cancellationToken: ct);
                if (result == null) throw new InvalidOperationException("Failed to deserialize OpenAlex references.");

                result.WorkId = workId;
                return result;
            }
            catch (Exception ex) when (ex is not BaseDomainException)
            {
                _logger.LogError(ex, "Error fetching references for work {WorkId} from OpenAlex", workId);
                throw;
            }
        }

        public async Task<CitationResultDto> GetCitationsAsync(
            string workId,
            int pageSize = 100,
            int? maxResults = null,
            CancellationToken ct = default)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            var works = new List<WorkSummaryDto>();
            var cursor = "*";
            int totalCount = 0;
            var select = "id,display_name,publication_year";

            try
            {
                while (cursor != null)
                {
                    var filter = $"cites:{workId}";
                    var url = $"works?filter={filter}&per_page={pageSize}&cursor={cursor}&select={select}&api_key={_settings.ApiKey}";

                    var response = await _httpClient.GetAsync(url, ct);
                    await EnsureSuccessAsync(response);

                    var pageResult = await response.Content.ReadFromJsonAsync<OpenAlexResponse<WorkSummaryDto>>(cancellationToken: ct);
                    if (pageResult == null) break;

                    if (works.Count == 0)
                    {
                        totalCount = pageResult.Meta.Count;
                    }

                    works.AddRange(pageResult.Results);

                    if (maxResults.HasValue && works.Count >= maxResults.Value)
                    {
                        if (works.Count > maxResults.Value)
                        {
                            works = works.GetRange(0, maxResults.Value);
                        }
                        break;
                    }

                    cursor = pageResult.Meta.NextCursor;
                }

                return new CitationResultDto
                {
                    TotalCount = totalCount,
                    Works = works
                };
            }
            catch (Exception ex) when (ex is not BaseDomainException)
            {
                _logger.LogError(ex, "Error fetching citations for work {WorkId} from OpenAlex", workId);
                throw;
            }
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("OpenAlex API error: {StatusCode} - {Content}", response.StatusCode, content);

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    throw new BadRequestException($"OpenAlex API Bad Request: {content}");
                case HttpStatusCode.Forbidden:
                    throw new ForbiddenException("OpenAlex API access forbidden. Check API key.");
                case HttpStatusCode.NotFound:
                    throw new NotFoundException("OpenAlex resource not found.");
                case HttpStatusCode.TooManyRequests:
                    // This should be handled by Polly, but if it reaches here:
                    throw new InvalidOperationException("OpenAlex API rate limit exceeded.");
                default:
                    throw new InvalidOperationException($"OpenAlex API error: {response.StatusCode}");
            }
        }
    }
}
