using System.Collections.Concurrent;
using System.Threading.Channels;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.SemanticScholar;

namespace SRSS.IAM.Services.SemanticScholar;

public class SemanticScholarJob
{
    public string CacheKey { get; set; } = null!;
    public SemanticScholarSearchRequest Request { get; set; } = null!;
    public TaskCompletionSource<PaginatedResponse<PaperSearchResultDto>> Tcs { get; set; } = new();
    public CancellationToken CancellationToken { get; set; }
}

public class SemanticScholarTaskQueue
{
    private readonly Channel<SemanticScholarJob> _queue;
    private readonly ConcurrentDictionary<string, Task<PaginatedResponse<PaperSearchResultDto>>> _activeTasks = new();
    private readonly SemanticScholarMetrics _metrics;

    public SemanticScholarTaskQueue(SemanticScholarMetrics metrics)
    {
        _metrics = metrics;
        // Unbounded channel as per requirement 3 (3000+ users)
        _queue = Channel.CreateUnbounded<SemanticScholarJob>();
    }

    public async Task<PaginatedResponse<PaperSearchResultDto>> EnqueueAsync(
        string cacheKey, 
        SemanticScholarSearchRequest request, 
        CancellationToken ct)
    {
        // Deduplication Layer (Requirement 4)
        if (_activeTasks.TryGetValue(cacheKey, out var existingTask))
        {
            return await existingTask;
        }

        var job = new SemanticScholarJob
        {
            CacheKey = cacheKey,
            Request = request,
            CancellationToken = ct
        };

        // Double-check locking for task creation
        var finalTask = _activeTasks.GetOrAdd(cacheKey, _ => 
        {
            _queue.Writer.TryWrite(job);
            _metrics.IncrementQueueSize();
            return job.Tcs.Task;
        });

        try
        {
            return await finalTask;
        }
        finally
        {
            // Remove from active tasks once completed so future requests can trigger new calls if cache expires
            // Note: This cleanup happens after the worker completes the TCS
            _activeTasks.TryRemove(cacheKey, out _);
        }
    }

    public async ValueTask<SemanticScholarJob> DequeueAsync(CancellationToken ct)
    {
        var job = await _queue.Reader.ReadAsync(ct);
        _metrics.DecrementQueueSize();
        return job;
    }
}
