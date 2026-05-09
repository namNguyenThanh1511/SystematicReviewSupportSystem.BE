using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ReviewChecklist : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public Guid TemplateId { get; set; }
        public bool IsCompleted { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }
        public string? PdfUrl { get; set; }
        public double? PageWidth { get; set; }
        public double? PageHeight { get; set; }

        public SystematicReviewProject Project { get; set; } = null!;
        public ChecklistTemplate Template { get; set; } = null!;
        public ICollection<ChecklistItemResponse> ItemResponses { get; set; } = new List<ChecklistItemResponse>();
    }
}
