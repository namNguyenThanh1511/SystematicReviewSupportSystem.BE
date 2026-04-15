using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class StudySelectionProcessPaper : BaseEntity<Guid>
    {
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }

        public bool IsAddedToDataset { get; set; } = false;

        // Navigation
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
    }
}