using System;
using SRSS.IAM.Services.DTOs.Crossref;

namespace SRSS.IAM.Services.DTOs.Identification
{
    public class ApiImportRequest
    {
        public CrossrefQueryParameters Query { get; set; } = new();
        public Guid? SearchSourceId { get; set; }
        public Guid ProjectId { get; set; }
    }
}
