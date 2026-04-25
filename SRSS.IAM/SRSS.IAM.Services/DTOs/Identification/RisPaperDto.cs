namespace SRSS.IAM.Services.DTOs.Identification
{
    public class RisPaperDto
    {
        public string Title { get; set; } = string.Empty;

        // Structured lists for better manipulation
        public List<string> AuthorList { get; set; } = new List<string>();
        public List<string> KeywordList { get; set; } = new List<string>();

        // Computed properties for backward compatibility and display
        public string? Authors => AuthorList.Count > 0 ? string.Join("; ", AuthorList) : null;
        public string? Keywords => KeywordList.Count > 0 ? string.Join("; ", KeywordList) : null;

        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? PublicationYear { get; set; }
        public DateTimeOffset? PublicationDate { get; set; }

        public string? Journal { get; set; }
        public string? JournalIssn { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Publisher { get; set; }

        // Extended Metadata for real-world RIS variations
        public string? BookTitle { get; set; }      // Tag: BT
        public string? SecondaryTitle { get; set; } // Tag: T2
        public string? ConferenceName { get; set; } // Tag: C3 (Primary)
        public string? ConferenceLocation { get; set; } // Tag: CY
        public string? ConferenceDate { get; set; } // Tag: CD or Y2
        public string? Id { get; set; }             // Tag: ID or AN

        public string? PublicationType { get; set; }
        public string? Url { get; set; }
        public string? RawReference { get; set; }
    }
}
