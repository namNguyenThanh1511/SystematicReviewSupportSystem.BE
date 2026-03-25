using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionProcessPaperService
{
    public interface IStudySelectionProcessPaperService
    {
        Task SaveFinalIncludedPapersAsync(Guid processId, CancellationToken cancellationToken);
        Task<List<IncludedPaperResponse>> GetIncludedPapersByProcessIdAsync(Guid processId, CancellationToken cancellationToken);
    }
}
