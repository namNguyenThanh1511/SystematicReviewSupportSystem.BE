namespace SRSS.IAM.Repositories.Entities.Enums
{
    public enum QualityAssessmentProcessStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
    public enum QualityAssessmentAssignmentStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }
    public enum QualityAssessmentDecisionValue
    {
        Yes = 0,
        No = 1,
        Unclear = 2,
    }
    public enum QualityAssessmentResolutionDecision
    {
        LowQuality = 0,
        HighQuality = 1
    }
}
