using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;

namespace SRSS.IAM.Services.ReferenceMatchingService
{
    public interface IReferenceMatchingService
    {
        Task<MatchResult> MatchAsync(ExtractedReference reference, Guid projectId, CancellationToken cancellationToken = default);
        Task<MatchResult?> FindSemanticMatchAsync(float[] embedding, Guid currentPaperId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MatchResult>> MatchBatchInProjectAsync(IEnumerable<ExtractedReference> references, Guid projectId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MatchResult>> MatchBatchAsync(IEnumerable<ExtractedReference> references, Guid identificationProcessId, CancellationToken cancellationToken = default);
        Task<MatchResult> MatchAgainstSnapshotAsync(ExtractedReference reference, Guid identificationProcessId, CancellationToken cancellationToken = default);
        MatchResult MatchAgainstProcessed(ExtractedReference reference, IEnumerable<ProcessedReference> processedReferences);
    }
}
