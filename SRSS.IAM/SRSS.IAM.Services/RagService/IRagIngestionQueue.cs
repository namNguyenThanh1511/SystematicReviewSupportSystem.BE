using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.RagService
{
    /// <summary>
    /// Queue for ingesting papers into the RAG pipeline asynchronously.
    /// </summary>
    public interface IRagIngestionQueue
    {
        /// <summary>
        /// Adds a paper processing job to the queue.
        /// </summary>
        ValueTask QueuePaperForIngestionAsync(Guid paperId, string pdfUrl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes and returns the next paper processing job from the queue.
        /// </summary>
        ValueTask<RagIngestionWorkItem> DequeueAsync(CancellationToken cancellationToken);
    }
}
