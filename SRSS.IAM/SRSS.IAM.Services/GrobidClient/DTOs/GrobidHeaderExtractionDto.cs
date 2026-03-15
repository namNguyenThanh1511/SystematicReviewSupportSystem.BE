namespace SRSS.IAM.Services.GrobidClient.DTOs
{
    public class GrobidHeaderExtractionDto
    {
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? Abstract { get; set; }
        public string? DOI { get; set; }
        public string? Journal { get; set; }
        public string? Volume { get; set; }
        public string? Issue { get; set; }
        public string? Pages { get; set; }
        public string? Publisher { get; set; }
        public DateTimeOffset? PublishedDate { get; set; }
        public int? Year { get; set; }
        public string? ISSN { get; set; }
        public string? EISSN { get; set; }
        public string? Keywords { get; set; }
        public string? Language { get; set; }
        public string? Md5 { get; set; }
        public string? RawXml { get; set; }
    }
}
