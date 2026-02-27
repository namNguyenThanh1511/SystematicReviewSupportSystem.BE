using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.ReviewProcess;
using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.DTOs.SystematicReviewProject
{
    public class CreateSystematicReviewProjectRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateSystematicReviewProjectRequest
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Domain { get; set; }
        public string? Description { get; set; }
    }

    public class SystematicReviewProjectResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public int TotalProcesses { get; set; }
        public int CompletedProcesses { get; set; }
        public List<ReviewProcessSummaryDto> Processes { get; set; } = new();
    }

    public class SystematicReviewProjectDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public List<ReviewProcessResponse> ReviewProcesses { get; set; } = new();
    }

    public class ReviewProcessSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ProcessStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public ProcessPhase CurrentPhase { get; set; }
        public string CurrentPhaseText { get; set; } = string.Empty;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }

    public class MyProjectResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public ProjectRole Role { get; set; }
        public string RoleText { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}

