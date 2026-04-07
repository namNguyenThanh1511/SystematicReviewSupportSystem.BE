using System.Text.Json;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public class StudySelectionAIResultService : IStudySelectionAIResultService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudySelectionAIResultService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StudySelectionAIResultResponse> GetByKeysAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase phase,
            CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.StudySelectionAIResults.GetByKeysAsync(
                studySelectionId,
                paperId,
                reviewerId,
                phase,
                cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException($"AI evaluation result not found for paper {paperId} and reviewer {reviewerId} in this phase.");
            }

            return new StudySelectionAIResultResponse
            {
                Id = entity.Id,
                StudySelectionProcessId = entity.StudySelectionProcessId,
                PaperId = entity.PaperId,
                ReviewerId = entity.ReviewerId,
                Phase = entity.Phase,
                AIOutput = !string.IsNullOrEmpty(entity.AIOutputJson) 
                    ? JsonSerializer.Deserialize<StuSeAIOutput>(entity.AIOutputJson) 
                    : null,
                RelevanceScore = entity.RelevanceScore,
                Recommendation = entity.Recommendation,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
        }
    }
}
