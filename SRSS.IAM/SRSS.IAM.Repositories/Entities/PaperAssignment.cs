using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperAssignment : BaseEntity<Guid>
    {
        public Guid PaperId { get; set; }
        public Guid ProjectMemberId { get; set; }

        // Navigation Properties
        public Paper Paper { get; set; } = null!;
        public ProjectMember ProjectMember { get; set; } = null!;

        public PaperAssignment()
        {
        }

        public PaperAssignment(Guid paperId, Guid projectMemberId)
        {
            Id = Guid.NewGuid();
            PaperId = paperId;
            ProjectMemberId = projectMemberId;
            CreatedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }
    }
}
