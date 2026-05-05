using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperFullTextParsedParagraph : BaseEntity<Guid>
    {
        public Guid SectionId { get; set; }
        public int Order { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Coordinates { get; set; }

        public virtual PaperFullTextParsedSection? Section { get; set; }
    }
}
