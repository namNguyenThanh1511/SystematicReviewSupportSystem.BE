using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ChecklistItemResponse : BaseEntity<Guid>
    {
        public Guid ReviewChecklistId { get; set; }
        public Guid ItemTemplateId { get; set; }
        public string? Content { get; set; }
        public string? Location { get; set; }
        public bool IsNotApplicable { get; set; }
        public bool IsReported { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }

        public ReviewChecklist ReviewChecklist { get; set; } = null!;
        public ChecklistItemTemplate ItemTemplate { get; set; } = null!;
    }
}
