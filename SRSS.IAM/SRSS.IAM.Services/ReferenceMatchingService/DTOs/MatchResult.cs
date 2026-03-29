using System;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.ReferenceMatchingService.DTOs
{
    public class MatchResult
    {
        public Paper? MatchedPaper { get; set; }
        // For duplicates within the same batch where the Paper entity might not be fully persisted yet
        public Guid? MatchedPaperId { get; set; } 
        public decimal ConfidenceScore { get; set; }
        public MatchStrategy Strategy { get; set; }
        public bool IsSnapshotDuplicate { get; set; }
    }
}
