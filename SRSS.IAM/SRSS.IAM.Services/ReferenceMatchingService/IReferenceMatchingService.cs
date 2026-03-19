using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;

namespace SRSS.IAM.Services.ReferenceMatchingService
{
    public interface IReferenceMatchingService
    {
        Task<MatchResult> MatchAsync(ExtractedReference reference, CancellationToken cancellationToken = default);
    }
}
