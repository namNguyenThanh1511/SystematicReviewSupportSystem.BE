using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.StudySelectionAIResultRepo
{
    public interface IStudySelectionAIResultRepository : IGenericRepository<StudySelectionAIResult, Guid, AppDbContext>
    {
        Task<StudySelectionAIResult?> GetByKeysAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default);
    }
}
