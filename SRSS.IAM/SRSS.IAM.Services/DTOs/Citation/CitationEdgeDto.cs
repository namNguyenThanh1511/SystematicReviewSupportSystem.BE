using System;

namespace SRSS.IAM.Services.DTOs.Citation
{
    public class CitationEdgeDto
    {
        public Guid SourcePaperId { get; set; }
        public Guid TargetPaperId { get; set; }
        public decimal ConfidenceScore { get; set; }
    }
}