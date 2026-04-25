using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.CandidatePaperService.DTOs;
using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.CandidatePaperService
{
    public interface ICandidatePaperService
    {
        Task ExtractReferencesFromPaperAsync(Guid paperId, CancellationToken cancellationToken = default);
        Task<PaginatedResponse<CandidatePaperDto>> GetCandidatePapersAsync(Guid paperId, GetCandidatePapersRequest request, CancellationToken cancellationToken = default);
        Task<PaginatedResponse<PaperWithCandidateDto>> GetPapersWithCandidatesAsync(Guid projectId, GetPapersRequest request, CancellationToken cancellationToken = default);
        Task<PaginatedResponse<CandidatePaperDto>> GetCandidatesByPaperIdAsync(Guid paperId, GetCandidatePapersRequest request, CancellationToken cancellationToken = default);
        Task RejectCandidatePapersAsync(RejectCandidatePaperRequest request, CancellationToken cancellationToken = default);
        Task SelectCandidatePapersAsync(SelectCandidatePaperRequest request, Guid userId, CancellationToken cancellationToken = default);

    }
}
