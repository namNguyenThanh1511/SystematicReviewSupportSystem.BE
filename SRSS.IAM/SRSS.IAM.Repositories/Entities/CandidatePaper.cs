using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class CandidatePaper : BaseEntity<Guid>
    {
        public Guid ReviewProcessId { get; set; }
        public Guid? OriginPaperId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? PublicationYear { get; set; }
        public string? DOI { get; set; }
        public string? RawReference { get; set; }
        public string? NormalizedReference { get; set; }
        public decimal ConfidenceScore { get; set; } = 0m;
        public decimal ExtractionQualityScore { get; set; } = 0m;
        public decimal MatchConfidenceScore { get; set; } = 0m;
        
        public ReferenceType ReferenceType { get; set; } = ReferenceType.Unknown;
        public CandidateStatus Status { get; set; } = CandidateStatus.Detected;

        public Guid? TargetPaperId { get; set; }
        public Guid? CitationId { get; set; }

        public bool IsSelectedInScreening { get; set; } = false;
        public DateTimeOffset? SelectedAt { get; set; }

        public string? ValidationNote { get; set; }

        // Navigation Properties
        public ReviewProcess ReviewProcess { get; set; } = null!;
        public Paper OriginPaper { get; set; } = null!;
        public Paper? TargetPaper { get; set; }
        public PaperCitation? Citation { get; set; }
    }
}
