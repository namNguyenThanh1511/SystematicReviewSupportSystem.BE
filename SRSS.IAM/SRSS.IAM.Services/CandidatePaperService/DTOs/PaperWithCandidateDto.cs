namespace SRSS.IAM.Services.CandidatePaperService.DTOs
{
    public class PaperWithCandidateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Authors { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        public string PublicationYear { get; set; } = string.Empty;
        public string DOI { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? PdfUrl { get; set; }
        public DateTimeOffset ImportedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public int CandidateCount { get; set; }
        public int SuggestedCount { get; set; }
        public int DuplicateCount { get; set; }
    }
}
