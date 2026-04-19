using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperFullTextParsedSection : BaseEntity<Guid>
    {
        public Guid PaperFullTextId { get; set; }
        public int Order { get; set; }
        public string SectionTitle { get; set; } = string.Empty;
        public string? SectionType { get; set; } // e.g., Abstract, Introduction, Methods, Results, Discussion, Conclusion, Other

        public virtual PaperFullText? PaperFullText { get; set; }
        public virtual ICollection<PaperFullTextParsedParagraph> Paragraphs { get; set; } = new List<PaperFullTextParsedParagraph>();
    }
}
