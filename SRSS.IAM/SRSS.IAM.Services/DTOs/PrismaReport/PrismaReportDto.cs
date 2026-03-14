using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.PrismaReport
{
    public class PrismaReportResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string? Notes { get; set; }
        public string? GeneratedBy { get; set; }
        public List<PrismaFlowRecordResponse> FlowRecords { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }

    public class PrismaFlowRecordResponse
    {
        public Guid Id { get; set; }
        public PrismaStage Stage { get; set; }
        public string StageText { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class GeneratePrismaReportRequest
    {
        public string? Notes { get; set; }
        public string? GeneratedBy { get; set; }
        public string Version { get; set; } = "1.0";
    }

    public class PrismaReportListResponse
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string? GeneratedBy { get; set; }
        public int TotalRecords { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
