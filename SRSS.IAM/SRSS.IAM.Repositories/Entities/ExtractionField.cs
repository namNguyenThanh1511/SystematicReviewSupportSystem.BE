using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class ExtractionField : BaseEntity<Guid>
    {
        public Guid SectionId { get; set; }

        public Guid? ParentFieldId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Instruction { get; set; }

        public FieldType FieldType { get; set; }

        public bool IsRequired { get; set; }

        public int OrderIndex { get; set; }

        // Navigation properties
        public ExtractionSection? Section { get; set; }
        public ExtractionField? ParentField { get; set; }
        public ICollection<ExtractionField> SubFields { get; set; } = new List<ExtractionField>();
        public ICollection<FieldOption> Options { get; set; } = new List<FieldOption>();
    }
}