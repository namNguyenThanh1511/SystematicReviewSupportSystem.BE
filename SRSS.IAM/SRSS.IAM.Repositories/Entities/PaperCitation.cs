using Shared.Entities.BaseEntity;
using System;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperCitation : BaseEntity<Guid>
    {
        public Guid SourcePaperId { get; set; }   // Paper A
        public Guid TargetPaperId { get; set; }   // Paper B

        public string? RawReference { get; set; }

        public decimal ConfidenceScore { get; set; }

        public CitationSource Source { get; set; }

        // Navigation properties
        public Paper SourcePaper { get; set; } = null!;
        public Paper TargetPaper { get; set; } = null!;
    }

    public enum CitationSource
    {
        Unknown = 0,
        DatabaseSearch = 1,
        Grobid = 2,
        UserManual = 3,
        Crossref = 4
    }
}
