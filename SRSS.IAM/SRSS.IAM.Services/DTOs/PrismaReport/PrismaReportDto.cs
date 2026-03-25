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
        public List<PrismaNodeResponse> Nodes { get; set; } = new();
        public PrismaNodeResponse? Included { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }

    public class PrismaNodeResponse
    {
        public string Stage { get; set; } = string.Empty;
        public int Total { get; set; }
        public List<PrismaBreakdownResponse>? Breakdown { get; set; }
        public List<PrismaBreakdownResponse>? Reasons { get; set; }
        public PrismaSideBoxResponse? SideBox { get; set; }
    }

    public class PrismaSideBoxResponse
    {
        public string Stage { get; set; } = string.Empty;
        public int Total { get; set; }
        public List<PrismaBreakdownResponse>? Breakdown { get; set; }
        public List<PrismaBreakdownResponse>? Reasons { get; set; }
    }

    public class PrismaBreakdownResponse
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
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
