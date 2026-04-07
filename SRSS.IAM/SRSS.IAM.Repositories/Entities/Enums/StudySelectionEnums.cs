namespace SRSS.IAM.Repositories.Entities.Enums
{
    /// <summary>
    /// Standardized exclusion reason codes per TA.md §5 (PRISMA-compliant taxonomy)
    /// Used for structured PRISMA reporting and exclusion reason breakdown
    /// </summary>
    public enum ExclusionReasonCode
    {
        NotRelevantToTopic = 0,
        NotRelevantPopulation = 1,
        NotRelevantIntervention = 2,
        NotEmpiricalStudy = 3,
        NotResearchPaper = 4,
        OutsideTimeRange = 5,
        UnsupportedLanguage = 6,
        DuplicateStudy = 7,
        Other = 99
    }

    /// <summary>
    /// Discriminator for screening phase (Title/Abstract vs Full-Text)
    /// </summary>
    public enum ScreeningPhase
    {
        TitleAbstract = 0,
        FullText = 1
    }

    /// <summary>
    /// Status for individual screening phases (TA or FT)
    /// </summary>
    public enum ScreeningPhaseStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }

    /// <summary>
    /// Recommendation provided by AI for a paper screening
    /// </summary>
    public enum StuSeAIRecommendation
    {
        Include = 0,
        Exclude = 1,
        Uncertain = 2
    }
}
