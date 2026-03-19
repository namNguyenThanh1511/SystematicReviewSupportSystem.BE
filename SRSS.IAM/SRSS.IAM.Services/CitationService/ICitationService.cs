using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.DTOs.Citation;

namespace SRSS.IAM.Services.CitationService
{
    public interface ICitationService
    {
        Task<List<PaperNodeDto>> GetReferencesAsync(Guid paperId, CancellationToken cancellationToken = default);
        Task<List<PaperNodeDto>> GetCitationsAsync(Guid paperId, CancellationToken cancellationToken = default);
        Task<int> GetCitationCountAsync(Guid paperId, CancellationToken cancellationToken = default);
        Task<int> GetReferenceCountAsync(Guid paperId, CancellationToken cancellationToken = default);
        Task<CitationGraphDto> GetCitationGraphAsync(Guid paperId, int depth, decimal minConfidence, CancellationToken cancellationToken = default);
        Task<List<PaperNodeDto>> GetTopCitedPapersAsync(int topN, CancellationToken cancellationToken = default);
        Task<List<PaperNodeDto>> GetSuggestedPapersAsync(Guid paperId, int limit = 5, CancellationToken cancellationToken = default);
    }
}
