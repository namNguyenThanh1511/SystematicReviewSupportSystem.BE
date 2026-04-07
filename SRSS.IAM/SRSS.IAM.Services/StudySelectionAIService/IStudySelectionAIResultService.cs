using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public interface IStudySelectionAIResultService
    {
        Task<StudySelectionAIResultResponse> GetByKeysAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default);
    }
}
