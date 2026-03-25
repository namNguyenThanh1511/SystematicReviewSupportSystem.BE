using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.Paper
{
    public class EnrichPaperResponseDto
    {
        public Guid PaperId { get; set; }
        public int? CitationCount { get; set; }
        public int? ReferenceCount { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BatchEnrichResponseDto
    {
        public int Total { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<EnrichPaperResponseDto> Results { get; set; } = new();
    }
}
