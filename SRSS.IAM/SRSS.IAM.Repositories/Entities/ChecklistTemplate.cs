using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ChecklistTemplate : BaseEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public string Version { get; set; } = "1.0";

        public ChecklistType Type { get; set; }

        // Alias to satisfy checklist requirement naming while reusing BaseEntity.ModifiedAt.
        public DateTimeOffset UpdatedAt
        {
            get => ModifiedAt;
            set => ModifiedAt = value;
        }

        public ICollection<ChecklistSectionTemplate> Sections { get; set; } = new List<ChecklistSectionTemplate>();
        public ICollection<ChecklistItemTemplate> ItemTemplates { get; set; } = new List<ChecklistItemTemplate>();
        public ICollection<ReviewChecklist> ReviewChecklists { get; set; } = new List<ReviewChecklist>();
    }

    public enum ChecklistType
    {
        Full = 0,
        Abstract = 1
    }
    
}
