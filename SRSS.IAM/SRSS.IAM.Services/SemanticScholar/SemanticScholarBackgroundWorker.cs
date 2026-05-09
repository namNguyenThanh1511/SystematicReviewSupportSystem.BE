using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Cache;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.SemanticScholar;
using System.Text.Json;

namespace SRSS.IAM.Services.SemanticScholar;

public class SemanticScholarBackgroundWorker : BackgroundService
{
    private readonly SemanticScholarTaskQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SemanticScholarBackgroundWorker> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly SemanticScholarMetrics _metrics;

    public SemanticScholarBackgroundWorker(
        SemanticScholarTaskQueue queue,
        IServiceProvider serviceProvider,
        ILogger<SemanticScholarBackgroundWorker> logger,
        IMemoryCache memoryCache,
        SemanticScholarMetrics metrics)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _memoryCache = memoryCache;
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Semantic Scholar Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.DequeueAsync(stoppingToken);

                // Use a dedicated scope for API client and Redis
                using (var scope = _serviceProvider.CreateScope())
                {
                    var apiClient = scope.ServiceProvider.GetRequiredService<ISemanticScholarApiClient>();
                    var redisCache = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();

                    try
                    {
                        var result = await apiClient.ExecuteSearchAsync(job.Request, job.CancellationToken);

                        // Populate Caches (Requirement 5)
                        // L1: 2 minutes
                        _memoryCache.Set(job.CacheKey, result, TimeSpan.FromMinutes(2));

                        // L2: 24 hours
                        await redisCache.SetAsync(job.CacheKey, JsonSerializer.Serialize(result), TimeSpan.FromHours(24));

                        job.Tcs.TrySetResult(result);
                    }
                    catch (OperationCanceledException)
                    {
                        job.Tcs.TrySetCanceled();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing Semantic Scholar job for key: {CacheKey}", job.CacheKey);
                        job.Tcs.TrySetException(ex);
                    }
                }

                // Mandatory Cooldown (Requirement 2 & 7: 1 req/sec hard cap)
                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in Semantic Scholar Background Worker loop.");
                await Task.Delay(5000, stoppingToken); // Wait a bit before retrying the loop
            }
        }

        _logger.LogInformation("Semantic Scholar Background Worker stopping.");
    }
}
