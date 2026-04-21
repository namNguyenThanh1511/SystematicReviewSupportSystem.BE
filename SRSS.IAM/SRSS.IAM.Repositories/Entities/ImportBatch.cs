using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ImportBatch : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? Source { get; set; }
        public int TotalRecords { get; set; }
        public string? ImportedBy { get; set; }
        public DateTimeOffset ImportedAt { get; set; }


        // Navigation Properties
        public SystematicReviewProject Project { get; set; } = null!;
        public ICollection<Paper> Papers { get; set; } = new List<Paper>();
    }
}
