using System;

namespace SRSS.IAM.Services.DTOs.Citation
{
    public class PaperNodeDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? Year { get; set; }
        public int CitationCount { get; set; }
        public string? Authors { get; set; }
        public string? Doi { get; set; }
    }
}