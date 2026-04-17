using System;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ExtractedDataAuditLogDto
    {
        public Guid Id { get; set; }
        public Guid PaperId { get; set; }
        public Guid FieldId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
