using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public class SimplifiedPaperResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Year { get; set; }
        public string? Source { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
        public List<ReviewerDecisionDto> AssignedReviewers { get; set; } = new();
    }

    public class ReviewerDecisionDto
    {
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty; // Pending, Included, Excluded
        public int? ExclusionReasonCode { get; set; }
        public string? ExclusionReasonName { get; set; }
        public string? ExclusionNote { get; set; }
    }
}
