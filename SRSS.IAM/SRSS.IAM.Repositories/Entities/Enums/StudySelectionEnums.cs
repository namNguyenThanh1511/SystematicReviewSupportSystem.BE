namespace SRSS.IAM.Repositories.Entities.Enums
{

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

    /// <summary>
    /// Filter for paper assignment status in screening phases
    /// </summary>
    public enum AssignmentFilterStatus
    {
        All = 0,
        Assigned = 1,
        Unassigned = 2
    }

    /// <summary>
    /// Filter for paper decision/resolution status in screening phases
    /// </summary>
    public enum ResolutionFilterStatus
    {
        All = 0,
        NotDecided = 1,
        Include = 2,
        Exclude = 3
    }

    /// <summary>
    /// Full-text retrieval state used for PRISMA-aligned tracking
    /// </summary>
    public enum FullTextRetrievalStatus
    {
        Unknown = 0,
        Retrieved = 1,
        NotRetrieved = 2
    }
    /// <summary>
    /// Source of an exclusion reason (Library vs Custom)
    /// </summary>
    public enum ExclusionReasonSource
    {
        Library = 0,
        Custom = 1
    }

    /// <summary>
    /// Filter for exclusion reason source
    /// </summary>
    public enum ExclusionReasonSourceFilter
    {
        All = 0,
        Library = 1,
        Custom = 2
    }
    /// <summary>
    /// Status for a checklist version
    /// </summary>
    public enum ChecklistVersionStatus
    {
        Draft = 0,
        Active = 1,
        Archived = 2
    }

    /// <summary>
    /// Type of input for a checklist item
    /// </summary>
    public enum ChecklistItemType
    {
        Boolean = 0,
        SingleChoice = 1,
        Text = 2
    }

    /// <summary>
    /// Impact of a checklist item on the screening decision
    /// </summary>
    public enum ChecklistItemImpact
    {
        Include = 0,
        Exclude = 1,
        Neutral = 2
    }

    /// <summary>
    /// Source of a checklist item (where it originated from)
    /// </summary>
    public enum ChecklistItemSourceType
    {
        Criteria = 0,
        ResearchQuestion = 1,
        PICOC = 2,
        Manual = 3
    }
}
