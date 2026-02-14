using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class Paper : BaseEntity<Guid>
    {
        // ============================================
        // CORE BIBLIOGRAPHIC METADATA
        // ============================================
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? PublicationType { get; set; }
        public string? PublicationYear { get; set; }
        public int? PublicationYearInt { get; set; }
        public DateTimeOffset? PublicationDate { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public string? AbstractLanguage { get; set; }
        public string? Keywords { get; set; }
        public string? Url { get; set; }
        public string? RawReference { get; set; }

        // ============================================
        // CONFERENCE METADATA
        // ============================================
        public string? ConferenceName { get; set; }
        public string? ConferenceLocation { get; set; }
        public string? ConferenceCountry { get; set; }
        public int? ConferenceYear { get; set; }
        public DateTimeOffset? ConferenceStartDate { get; set; }
        public DateTimeOffset? ConferenceEndDate { get; set; }

        // ============================================
        // JOURNAL METADATA
        // ============================================
        public string? Journal { get; set; }
        public string? JournalIssn { get; set; }
        public string? JournalEIssn { get; set; }
        public string? JournalPublisher { get; set; }

        // ============================================
        // IMPORT & SOURCE TRACKING (IDENTIFICATION PHASE)
        // ============================================
        public string? Source { get; set; }
        public string? SourceRecordId { get; set; }
        public Guid? ImportBatchId { get; set; }
        public DateTimeOffset? ImportedAt { get; set; }
        public string? ImportedBy { get; set; }

        // ============================================
        // ACCESS & FULL-TEXT AVAILABILITY
        // ============================================
        public string? PdfUrl { get; set; }
        public bool? FullTextAvailable { get; set; }
        public AccessType? AccessType { get; set; }

        // ============================================
        // AUDIT FIELDS (NO WORKFLOW STATE)
        // ============================================
        public string? InternalNotes { get; set; }

        // ============================================
        // NAVIGATION PROPERTIES
        // ============================================
        public ImportBatch? ImportBatch { get; set; }

        public Guid ProjectId { get; set; }
        public SystematicReviewProject? Project { get; set; }

        public ICollection<ScreeningDecision> ScreeningDecisions { get; set; } = new List<ScreeningDecision>();
        public ICollection<ScreeningResolution> ScreeningResolutions { get; set; } = new List<ScreeningResolution>();

        // Deduplication results where this paper IS the duplicate
        public ICollection<DeduplicationResult> DuplicateResults { get; set; } = new List<DeduplicationResult>();

        // Deduplication results where this paper is the ORIGINAL
        public ICollection<DeduplicationResult> OriginalOfDuplicates { get; set; } = new List<DeduplicationResult>();
    }

    public enum AccessType
    {
        Unknown = 0,
        OpenAccess = 1,
        Subscription = 2,
        Hybrid = 3,
        Bronze = 4,
        Green = 5,
        Gold = 6
    }
}
