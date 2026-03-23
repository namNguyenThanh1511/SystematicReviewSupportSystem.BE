using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;

namespace SRSS.IAM.Services.ReferenceMatchingService
{
    public interface IReferenceMatchingService
    {
        Task<MatchResult> MatchAsync(ExtractedReference reference, CancellationToken cancellationToken = default);
        Task<IEnumerable<MatchResult>> MatchBatchAsync(IEnumerable<ExtractedReference> references, Guid identificationProcessId, CancellationToken cancellationToken = default);
        MatchResult MatchAgainstProcessed(ExtractedReference reference, IEnumerable<ProcessedReference> processedReferences);
    }
}
