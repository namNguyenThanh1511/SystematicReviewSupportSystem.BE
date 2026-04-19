using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public interface IStuSeFullTextAiEvaluationService
    {
        Task<StuSeAIOutput> EvaluateFullTextAsync(
            Guid studySelectionId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default);
    }
}
