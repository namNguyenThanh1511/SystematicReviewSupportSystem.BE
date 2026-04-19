using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.PaperFullTextService
{
        public interface IPaperFullTextService
        {
                /// <summary>
                /// Extracts full-text from a PDF and stores it in the database.
                /// </summary>
                /// <param name="paperPdfId">The ID of the PaperPdf record.</param>
                /// <param name="cancellationToken">Cancellation token.</param>
                Task ExtractAndStoreFullTextAsync(Guid paperPdfId, CancellationToken cancellationToken = default);
                Task ParseFullTextAsync(Guid paperPdfId, CancellationToken cancellationToken = default);

                /// <summary>
                /// Extracts full-text using a versioned work item so stale jobs can be rejected.
                /// </summary>
                Task ExtractAndStoreFullTextAsync(PaperFullTextWorkItem workItem, CancellationToken cancellationToken = default);
        }
}
