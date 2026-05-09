using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shared.Cache;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.SemanticScholar;
using System.Text.Json;

namespace SRSS.IAM.Services.SemanticScholar;

public class SemanticScholarService : ISemanticScholarService
{
    private readonly SemanticScholarTaskQueue _queue;
    private readonly IMemoryCache _memoryCache;
    private readonly IRedisCacheService _redisCache;
    private readonly SemanticScholarMetrics _metrics;
    private readonly ILogger<SemanticScholarService> _logger;

    public SemanticScholarService(
        SemanticScholarTaskQueue queue,
        IMemoryCache memoryCache,
        IRedisCacheService redisCache,
        SemanticScholarMetrics metrics,
        ILogger<SemanticScholarService> logger)
    {
        _queue = queue;
        _memoryCache = memoryCache;
        _redisCache = redisCache;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PaperSearchResultDto>> SearchPapersAsync(SemanticScholarSearchRequest request, CancellationToken ct = default)
    {
        var cacheKey = GenerateCacheKey(request);

        // 1. Check L1 Cache (Requirement 5.1 & 6)
        if (_memoryCache.TryGetValue(cacheKey, out PaginatedResponse<PaperSearchResultDto>? l1Result))
        {
            _metrics.RecordCacheHitL1();
            _logger.LogInformation("Semantic Scholar: L1 Cache Hit for key: {CacheKey}", cacheKey);
            return l1Result!;
        }

        // 2. Check L2 Cache (Requirement 5.2 & 6)
        try
        {
            var l2Json = await _redisCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(l2Json))
            {
                var l2Result = JsonSerializer.Deserialize<PaginatedResponse<PaperSearchResultDto>>(l2Json);
                if (l2Result != null)
                {
                    _metrics.RecordCacheHitL2();
                    _logger.LogInformation("Semantic Scholar: L2 Cache Hit for key: {CacheKey}", cacheKey);
                    
                    // Populate L1 for future "hot" access
                    _memoryCache.Set(cacheKey, l2Result, TimeSpan.FromMinutes(2));
                    
                    // Fix legacy cached results with "UNKNOWN" status (Requirement 7)
                    foreach (var paper in l2Result.Items.Where(p => p.PdfStatus == "UNKNOWN"))
                    {
                        paper.PdfStatus = PaperSearchResultDto.ClassifyPdfStatus(paper.OpenAccessPdfUrl);
                    }

                    return l2Result;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading from L2 Cache for key: {CacheKey}", cacheKey);
        }

        // 3. Cache Miss -> Enqueue to Rate-Limited Worker (Requirement 3 & 6)
        _metrics.RecordCacheMiss();
        _logger.LogInformation("Semantic Scholar: Cache Miss. Enqueueing request for key: {CacheKey}", cacheKey);
        
        return await _queue.EnqueueAsync(cacheKey, request, ct);
    }

    private string GenerateCacheKey(SemanticScholarSearchRequest request)
    {
        // Cache key: keyword + yearFrom + yearTo + page + pageSize (Requirement 5)
        return $"ss_search:{request.Keyword.ToLower().Trim()}:y{request.YearFrom}-{request.YearTo}:p{request.Page}:s{request.PageSize}";
    }
}
