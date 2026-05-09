namespace SRSS.IAM.Services.SemanticScholar;

public class SemanticScholarMetrics
{
    private long _queueSize;
    private long _cacheHitsL1;
    private long _cacheHitsL2;
    private long _cacheMisses;
    private long _externalCalls;
    private long _failedCalls;
    private long _totalProcessingTimeMs;

    public long QueueSize => Interlocked.Read(ref _queueSize);
    public long CacheHitsL1 => Interlocked.Read(ref _cacheHitsL1);
    public long CacheHitsL2 => Interlocked.Read(ref _cacheHitsL2);
    public long CacheMisses => Interlocked.Read(ref _cacheMisses);
    public long ExternalCalls => Interlocked.Read(ref _externalCalls);
    public long FailedCalls => Interlocked.Read(ref _failedCalls);
    public double AvgProcessingTimeMs => ExternalCalls == 0 ? 0 : (double)Interlocked.Read(ref _totalProcessingTimeMs) / ExternalCalls;

    public void IncrementQueueSize() => Interlocked.Increment(ref _queueSize);
    public void DecrementQueueSize() => Interlocked.Decrement(ref _queueSize);
    public void RecordCacheHitL1() => Interlocked.Increment(ref _cacheHitsL1);
    public void RecordCacheHitL2() => Interlocked.Increment(ref _cacheHitsL2);
    public void RecordCacheMiss() => Interlocked.Increment(ref _cacheMisses);
    public void RecordExternalCall(long durationMs)
    {
        Interlocked.Increment(ref _externalCalls);
        Interlocked.Add(ref _totalProcessingTimeMs, durationMs);
    }
    public void RecordFailedCall() => Interlocked.Increment(ref _failedCalls);
}
