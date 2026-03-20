using System;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.ReferenceMatchingService.DTOs
{
    public class MatchResult
    {
        public Paper? MatchedPaper { get; set; }
        public decimal ConfidenceScore { get; set; }
        public MatchStrategy Strategy { get; set; }
    }
}
