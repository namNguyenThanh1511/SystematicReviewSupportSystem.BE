using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperAssignment : BaseEntity<Guid>
    {
        public Guid PaperId { get; set; }

        public Guid ProjectMemberId { get; set; }

        public Guid StudySelectionProcessId { get; set; }

        public ScreeningPhase Phase { get; set; }

        // Navigation Properties
        public Paper Paper { get; set; } = null!;

        public ProjectMember ProjectMember { get; set; } = null!;

        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;

        public PaperAssignment()
        {
        }

        public PaperAssignment(
            Guid paperId,
            Guid projectMemberId,
            Guid studySelectionProcessId,
            ScreeningPhase phase)
        {
            Id = Guid.NewGuid();
            PaperId = paperId;
            ProjectMemberId = projectMemberId;
            StudySelectionProcessId = studySelectionProcessId;
            Phase = phase;

            CreatedAt = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
            ModifiedAt = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
        }

    }
}
