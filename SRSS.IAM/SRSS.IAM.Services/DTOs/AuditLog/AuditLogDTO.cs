using System;
using System.Text.Json;

namespace SRSS.IAM.Services.DTOs.AuditLog
{
    public class AuditLogResponse
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        public string Action { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        
        public string ResourceType { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        
        public string Status { get; set; } = "Success";
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string Importance { get; set; } = "low";
        
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        
        public object? AffectedColumns { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ReviewProcessId { get; set; }
    }
}