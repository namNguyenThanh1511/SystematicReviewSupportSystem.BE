using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.AuditLogRepo
{
    public class AuditEntry
    {
        public EntityEntry Entry { get; }
        public string ResourceType { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public Dictionary<string, object?> OldValue { get; } = new();
        public Dictionary<string, object?> NewValue { get; } = new();
        public List<string> ChangedColumns { get; } = new();
        public string ActionType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Guid? ProjectId { get; set; }

        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public AuditLog ToAuditLog(string userId, string userName = "", string? ipAddress = null, string? userAgent = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                UserName = userName,
                Action = string.IsNullOrEmpty(Action) ? $"{ActionType} on {ResourceType}" : Action,
                ActionType = ActionType.ToLower(),
                ResourceType = ResourceType,
                ResourceId = ResourceId,
                Status = "Success",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Importance = ActionType == "Delete" ? "high" : "medium",
                OldValue = OldValue.Count == 0 ? null : JsonSerializer.Serialize(OldValue),
                NewValue = NewValue.Count == 0 ? null : JsonSerializer.Serialize(NewValue),
                AffectedColumns = ChangedColumns.Count == 0 ? string.Empty : JsonSerializer.Serialize(ChangedColumns),
                ProjectId = ProjectId
            };

            return auditLog;
        }
    }
}
