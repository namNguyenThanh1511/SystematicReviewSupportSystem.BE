using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ExtractedDataAuditLog : BaseEntity<Guid>
    {
        public Guid ExtractionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public Guid FieldId { get; set; }
        public Guid? MatrixColumnId { get; set; }
        public int? MatrixRowIndex { get; set; }
        public Guid UserId { get; set; }

        public string OldValue { get; set; } = null!;
        public string NewValue { get; set; } = null!;

        // Navigation properties
        public DataExtractionProcess ExtractionProcess { get; set; } = null!;
        public Paper Paper { get; set; } = null!;
        public ExtractionField Field { get; set; } = null!;
        public ExtractionMatrixColumn? MatrixColumn { get; set; }
        public User User { get; set; } = null!;
    }
}
