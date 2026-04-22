using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperFullText : BaseEntity<Guid>
    {
        public Guid PaperPdfId { get; set; }
        public string RawXml { get; set; } = string.Empty; // TEI XML content from GROBID
        public DateTimeOffset? ParsedAt { get; set; }
        public DateTimeOffset? ChunkedAt { get; set; }
        public DateTimeOffset? EmbeddedAt { get; set; }

        public PaperPdf? PaperPdf { get; set; }
        public virtual ICollection<PaperFullTextParsedSection> ParsedSections { get; set; } = new List<PaperFullTextParsedSection>();
        public virtual ICollection<PaperFullTextChunk> Chunks { get; set; } = new List<PaperFullTextChunk>();
    }
}
