using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class AuditLog : BaseEntity<Guid>
    {
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
        
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        
        public string AffectedColumns { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Guid? ProjectId { get; set; }
        public Guid? ReviewProcessId { get; set; }
    }
}
