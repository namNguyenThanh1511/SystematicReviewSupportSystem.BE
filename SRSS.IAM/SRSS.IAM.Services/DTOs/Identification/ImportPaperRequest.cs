namespace SRSS.IAM.Services.DTOs.Identification
{
    public class ImportPaperRequest
    {
        public Guid? SearchExecutionId { get; set; }
        public string? ImportedBy { get; set; }
        public List<PaperDto> Papers { get; set; } = new();
    }

    public class PaperDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? PublicationYear { get; set; }
        public string? Journal { get; set; }
        public string? Url { get; set; }
        public string? Keywords { get; set; }
    }
}

