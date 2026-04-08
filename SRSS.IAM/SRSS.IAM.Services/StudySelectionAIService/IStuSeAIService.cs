using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public interface IStuSeAIService
    {
        Task<StuSeAIOutput> GetAiEvaluationAsync(StuSeAIInput input);

        Task<StuSeAIOutput> EvaluateTitleAbstractAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default
        );
    }
}
