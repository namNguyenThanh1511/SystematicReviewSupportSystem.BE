namespace SRSS.IAM.Services.DTOs.Identification
{
    public class RisPaperDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
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
        public string? ConferenceLocation { get; set; }
        public string? ConferenceName { get; set; }
        public string? PublicationType { get; set; }
        public string? Url { get; set; }
        public string? Keywords { get; set; }
        public string? RawReference { get; set; }
    }
}
