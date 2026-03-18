using System;

namespace SRSS.IAM.Services.ReferenceMatchingService.DTOs
{
    public class ExtractedReference
    {
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? DOI { get; set; }
        public string? PublishedYear { get; set; }
        public string? RawReference { get; set; }
    }
}
