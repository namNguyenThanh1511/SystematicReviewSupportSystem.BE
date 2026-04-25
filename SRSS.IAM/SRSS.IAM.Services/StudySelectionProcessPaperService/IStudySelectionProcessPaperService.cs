using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionProcessPaperService
{
    public interface IStudySelectionProcessPaperService
    {
        Task SaveFinalIncludedPapersAsync(Guid processId, CancellationToken cancellationToken);
        Task<PaginatedResponse<IncludedPaperResponse>> GetIncludedPapersByProcessIdAsync(Guid processId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<PaginatedResponse<IncludedPaperResponse>> GetIncludedPapersByReviewProcessIdAsync(Guid reviewProcessId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task SaveMultipleIncludedPapersInFullTextPhaseAsync(Guid processId, List<Guid> paperIds, CancellationToken cancellationToken);
    }
}
