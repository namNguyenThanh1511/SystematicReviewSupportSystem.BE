using System;

namespace SRSS.IAM.Services.ReferenceMatchingService.DTOs
{
    public class ProcessedReference
    {
        public ExtractedReference Reference { get; set; } = null!;
        public Guid PaperId { get; set; }
    }
}
