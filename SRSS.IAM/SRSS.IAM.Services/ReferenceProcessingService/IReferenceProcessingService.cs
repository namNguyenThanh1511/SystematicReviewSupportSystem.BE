using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.ReferenceProcessingService
{
    public interface IReferenceProcessingService
    {
        /// <summary>
        /// Processes all detected candidates for a given paper: matches references,
        /// creates papers for unmatched references, and always creates citations.
        /// </summary>
        Task ProcessCandidatesAsync(Guid processId, Guid paperId, CancellationToken cancellationToken = default);
    }
}
