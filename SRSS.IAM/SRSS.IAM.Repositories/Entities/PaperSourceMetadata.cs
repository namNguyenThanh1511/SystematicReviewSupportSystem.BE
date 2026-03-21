using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperSourceMetadata : BaseEntity<Guid>
    {
        public Guid PaperId { get; set; }
        public MetadataSource Source { get; set; }

        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? Journal { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Keywords { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public int? Year { get; set; }
        public string? ISSN { get; set; }
        public string? EISSN { get; set; }
        public string? Language { get; set; }
        public string? Md5 { get; set; }
        public List<string> AppliedFields { get; set; } = new List<string>();
        public DateTimeOffset ExtractedAt { get; set; }
        public Paper? Paper { get; set; }
    }
}
