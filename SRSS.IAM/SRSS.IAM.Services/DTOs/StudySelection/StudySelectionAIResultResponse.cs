using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.StudySelection
{
    public class StudySelectionAIResultResponse
    {
        public Guid Id { get; set; }
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public Guid ReviewerId { get; set; }
        public ScreeningPhase Phase { get; set; }
        public StuSeAIOutput? AIOutput { get; set; }
        public double RelevanceScore { get; set; }
        public StuSeAIRecommendation Recommendation { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
