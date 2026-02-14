using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.Paper;

namespace SRSS.IAM.Services.PaperService
{
    public interface IPaperService
    {
        Task<PaginatedResponse<PaperResponse>> GetPapersByProjectAsync(
            Guid projectId,
            PaperListRequest request,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperResponse>> GetDuplicatePapersByProjectAsync(
            Guid projectId,
            DuplicatePapersRequest request,
            CancellationToken cancellationToken = default);

        Task<PaperResponse> GetPaperByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaperResponse>> SearchPapersAsync(
            Guid projectId,
            PaperSearchRequest request,
            CancellationToken cancellationToken = default);
    }
}
