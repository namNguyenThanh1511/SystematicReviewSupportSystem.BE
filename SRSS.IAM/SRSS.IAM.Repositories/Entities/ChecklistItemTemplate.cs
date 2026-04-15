using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ChecklistItemTemplate : BaseEntity<Guid>
    {
        public Guid TemplateId { get; set; }
        public Guid? ParentId { get; set; }
        public string ItemNumber { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool HasLocationField { get; set; } = true;
        public string? DefaultSampleAnswer { get; set; }

        public ChecklistTemplate Template { get; set; } = null!;
        public ChecklistItemTemplate? Parent { get; set; }
        public ICollection<ChecklistItemTemplate> Children { get; set; } = new List<ChecklistItemTemplate>();
        public ICollection<ChecklistItemResponse> Responses { get; set; } = new List<ChecklistItemResponse>();
    }
}
