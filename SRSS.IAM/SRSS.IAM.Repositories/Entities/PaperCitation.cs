using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;
using System;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperCitation : BaseEntity<Guid>
    {
        public Guid SourcePaperId { get; set; }   // Paper A
        public Guid? TargetPaperId { get; set; }   // Paper B (Optional if non-paper)
        public ReferenceType ReferenceType { get; set; } // NEW

        public string? RawReference { get; set; }

        public decimal ConfidenceScore { get; set; }

        public decimal ExtractionQuality { get; set; }

        public decimal MatchConfidence { get; set; }

        public CitationSource Source { get; set; }

        public bool IsExternal { get; set; }

        public string? ExternalId { get; set; }

        public decimal? Weight { get; set; }

        public bool IsLowConfidence { get; set; }

        // Navigation properties
        public Paper SourcePaper { get; set; } = null!;
        public Paper? TargetPaper { get; set; }
    }

    public enum CitationSource
    {
        Unknown = 0,
        DatabaseSearch = 1,
        Grobid = 2,
        UserManual = 3,
        Crossref = 4,
        OpenAlex = 5
    }
}
