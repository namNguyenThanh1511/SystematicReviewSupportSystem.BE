using System;

namespace SRSS.IAM.Services.DTOs.SystematicReviewProject
{
    public class ProjectPicocResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string? Population { get; set; }
        public string? Intervention { get; set; }
        public string? Comparator { get; set; }
        public string? Outcome { get; set; }
        public string? Context { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
