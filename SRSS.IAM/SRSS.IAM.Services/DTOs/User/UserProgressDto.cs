using SRSS.IAM.Repositories.Entities.Enums;
using System;

namespace SRSS.IAM.Services.DTOs.User
{
    public class UserProgressOverviewResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Workload { get; set; }
        public int Completed { get; set; }
        public double Progress { get; set; }
        public ReviewerStatus Status { get; set; }
        public string StatusText => Status.ToString();
        public DateTimeOffset LastSynchronizedAt { get; set; }
    }

    public enum ReviewerStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

    public class UserProgressRequest
    {
        public Guid ProjectId { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
