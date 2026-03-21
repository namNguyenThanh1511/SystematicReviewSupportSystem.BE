using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class GrobidHeaderResult : BaseEntity<Guid>
    {
        public Guid PaperPdfId { get; set; }

        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? Journal { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }

        public string? RawXml { get; set; }

        public DateTimeOffset ExtractedAt { get; set; }

        public PaperPdf? PaperPdf { get; set; }
    }
}
