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
    }
}
