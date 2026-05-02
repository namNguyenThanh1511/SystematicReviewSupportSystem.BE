using System;

namespace SRSS.IAM.Services.DTOs.Identification
{
    public class DoiImportRequest
    {
        public string Doi { get; set; } = string.Empty;
        public Guid? SearchSourceId { get; set; }
        public Guid ProjectId { get; set; }
    }
}
