using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the final resolved decision for a paper in a selection process
    /// One resolution per paper per selection process per phase
    /// </summary>
    public class ScreeningResolution : BaseEntity<Guid>
    {
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public ScreeningDecisionType FinalDecision { get; set; }
        public ScreeningPhase Phase { get; set; } = ScreeningPhase.TitleAbstract;
        public string? ResolutionNotes { get; set; }
        public Guid ResolvedBy { get; set; }
        public DateTimeOffset ResolvedAt { get; set; }

        // Navigation Properties
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
    }
}
