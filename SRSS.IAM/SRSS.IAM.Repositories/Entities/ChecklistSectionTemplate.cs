using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ChecklistSectionTemplate : BaseEntity<Guid>
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public string SectionNumber { get; set; } = string.Empty;

        public ChecklistTemplate Template { get; set; } = null!;
        public ICollection<ChecklistItemTemplate> ItemTemplates { get; set; } = new List<ChecklistItemTemplate>();
    }
}