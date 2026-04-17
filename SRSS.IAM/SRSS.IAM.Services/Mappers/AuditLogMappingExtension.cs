using System;
using System.Text.Json;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.AuditLog;

namespace SRSS.IAM.Services.Mappers
{
    public static class AuditLogMappingExtension
    {
        public static AuditLogResponse ToResponse(this AuditLog entity)
        {
            return new AuditLogResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserName = entity.UserName,
                Action = entity.Action,
                ActionType = entity.ActionType,
                ResourceType = entity.ResourceType,
                ResourceId = entity.ResourceId,
                Status = entity.Status,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                Importance = entity.Importance,
                OldValue = ParseJsonValue(entity.OldValue),
                NewValue = ParseJsonValue(entity.NewValue),
                AffectedColumns = ParseJsonValue(entity.AffectedColumns),
                Timestamp = entity.Timestamp,
                ProjectId = entity.ProjectId
            };
        }

        private static object? ParseJsonValue(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                // Deserialize to a dynamic object or JsonDocument
                var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind == JsonValueKind.Array || document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    return JsonSerializer.Deserialize<object>(json);
                }
                
                // Or try checking string
                return document.RootElement.ToString();
            }
            catch
            {
                return json; // Fallback to raw string if not JSON
            }
        }
    }
}
