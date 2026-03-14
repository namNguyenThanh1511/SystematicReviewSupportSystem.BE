using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ImportBatch : BaseEntity<Guid>
    {
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? Source { get; set; }
        public int TotalRecords { get; set; }
        public string? ImportedBy { get; set; }
        public DateTimeOffset ImportedAt { get; set; }

        public Guid? SearchExecutionId { get; set; } //nullable to allow for manual imports not linked to a search execution

        public SearchExecution? SearchExecution { get; set; }

        // Navigation Properties
        public ICollection<Paper> Papers { get; set; } = new List<Paper>();
    }
}
