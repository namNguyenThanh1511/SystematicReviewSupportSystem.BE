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
        Task ExtractReferencesFromPaperAsync(Guid processId, Guid paperId, CancellationToken cancellationToken = default);
        Task<PaginatedResponse<CandidatePaperDto>> GetCandidatePapersAsync(Guid processId, GetCandidatePapersRequest request, CancellationToken cancellationToken = default);
        Task RejectCandidatePapersAsync(Guid processId, RejectCandidatePaperRequest request, CancellationToken cancellationToken = default);
        Task SelectCandidatePapersAsync(Guid processId, SelectCandidatePaperRequest request, Guid userId, CancellationToken cancellationToken = default);
    }
}
