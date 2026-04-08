using System.Text.Json;
using SRSS.IAM.Repositories.Entities;
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

        public async Task SaveAIResultAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase phase,
            StuSeAIOutput aiOutput,
            CancellationToken cancellationToken)
        {
            var existingResult = await _unitOfWork.StudySelectionAIResults.GetByKeysAsync(studySelectionId, paperId, reviewerId, phase, cancellationToken);

            if (existingResult != null)
            {
                existingResult.AIOutputJson = JsonSerializer.Serialize(aiOutput);
                existingResult.RelevanceScore = aiOutput.RelevanceScore;
                existingResult.Recommendation = MapRecommendation(aiOutput.Recommendation);
                existingResult.ModifiedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.StudySelectionAIResults.UpdateAsync(existingResult, cancellationToken);
            }
            else
            {
                var newResult = new StudySelectionAIResult
                {
                    Id = Guid.NewGuid(),
                    StudySelectionProcessId = studySelectionId,
                    PaperId = paperId,
                    ReviewerId = reviewerId,
                    Phase = phase,
                    AIOutputJson = JsonSerializer.Serialize(aiOutput),
                    RelevanceScore = aiOutput.RelevanceScore,
                    Recommendation = MapRecommendation(aiOutput.Recommendation),
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };
                await _unitOfWork.StudySelectionAIResults.AddAsync(newResult, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private StuSeAIRecommendation MapRecommendation(string recommendation)
        {
            return recommendation switch
            {
                "Include" => StuSeAIRecommendation.Include,
                "Exclude" => StuSeAIRecommendation.Exclude,
                _ => StuSeAIRecommendation.Uncertain
            };
        }
    }
}
