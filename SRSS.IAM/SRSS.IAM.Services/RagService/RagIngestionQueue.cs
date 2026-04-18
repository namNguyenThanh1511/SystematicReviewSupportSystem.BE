using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.RagService
{
    /// <summary>
    /// Channel-backed implementation of IRagIngestionQueue.
    /// Registered as a Singleton to allow controllers to enqueue jobs and the 
    /// BackgroundService to consume them concurrently.
    /// </summary>
    public class RagIngestionQueue : IRagIngestionQueue
    {
        private readonly Channel<RagIngestionWorkItem> _queue;

        public RagIngestionQueue()
        {
            // Bounded channel to prevent backpressure from causing OOM if ingestion spikes
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<RagIngestionWorkItem>(options);
        }

        public async ValueTask QueuePaperForIngestionAsync(Guid paperId, string pdfUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl))
            {
                throw new ArgumentException("PDF URL cannot be empty.", nameof(pdfUrl));
            }

            var workItem = new RagIngestionWorkItem(paperId, pdfUrl);
            await _queue.Writer.WriteAsync(workItem, cancellationToken);
        }

        public async ValueTask<RagIngestionWorkItem> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
