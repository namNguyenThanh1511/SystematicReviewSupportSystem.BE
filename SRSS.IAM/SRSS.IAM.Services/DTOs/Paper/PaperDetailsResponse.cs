using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.DTOs.Paper
{
    public class PaperDetailsResponse
    {
        public Guid Id { get; set; }
        // Core Metadata
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
        public string? Keywords { get; set; }
        public string? Url { get; set; }

        // Conference Metadata
        public string? ConferenceName { get; set; }
        public string? ConferenceLocation { get; set; }
        public string? ConferenceCountry { get; set; }
        public int? ConferenceYear { get; set; }

        // Journal Metadata
        public string? Journal { get; set; }
        public string? JournalIssn { get; set; }

        public string? JournalEIssn { get; set; }

        public string? Md5 { get; set; }

        // Source Tracking
        public string? Source { get; set; }
        public Guid? SearchSourceId { get; set; }
        public DateTimeOffset? ImportedAt { get; set; }
        public string? ImportedBy { get; set; }
        public ExtractionSuggestionResponse? ExtractionSuggestion { get; set; }
        // Access
        public string? PdfUrl { get; set; }
        public FullTextRetrievalStatus FullTextRetrievalStatus { get; set; }
        public string FullTextRetrievalStatusText { get; set; } = string.Empty;
        public bool? FullTextAvailable { get; set; }
        // Audit
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }


    }

}