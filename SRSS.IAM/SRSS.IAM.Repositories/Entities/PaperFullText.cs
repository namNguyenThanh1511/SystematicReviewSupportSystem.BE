using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperFullText : BaseEntity<Guid>
    {
        public Guid PaperPdfId { get; set; }
        public string RawXml { get; set; } = string.Empty; // TEI XML content from GROBID

        public PaperPdf? PaperPdf { get; set; }
    }
}
