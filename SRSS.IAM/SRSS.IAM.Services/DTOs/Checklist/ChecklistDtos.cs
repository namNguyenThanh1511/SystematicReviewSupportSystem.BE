namespace SRSS.IAM.Services.DTOs.Checklist
{
    public class ChecklistTemplateSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public string Version { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class ChecklistTemplateDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<ChecklistItemTemplateDto> Items { get; set; } = new();
    }

    public class ChecklistItemTemplateDto
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public Guid? ParentId { get; set; }
        public string ItemNumber { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public bool HasLocationField { get; set; }
        public string? DefaultSampleAnswer { get; set; }
    }

    public class CreateChecklistTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Version { get; set; } = "1.0";
        public List<CreateChecklistItemTemplateDto> Items { get; set; } = new();
    }

    public class CreateChecklistItemTemplateDto
    {
        public string ItemNumber { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsRequired { get; set; } = true;
        public bool HasLocationField { get; set; } = true;
        public string? DefaultSampleAnswer { get; set; }
        public string? ParentItemNumber { get; set; }
    }

    public class CloneChecklistRequestDto
    {
        public Guid TemplateId { get; set; }
    }

    public class UpdateChecklistItemDto
    {
        public string? Content { get; set; }
        public string? Location { get; set; }
        public bool IsNotApplicable { get; set; }
        public bool IsReported { get; set; }
    }

    public class ChecklistItemResponseDto
    {
        public Guid ItemTemplateId { get; set; }
        public Guid? ResponseId { get; set; }
        public Guid? ParentId { get; set; }
        public string ItemNumber { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsRequired { get; set; }
        public bool HasLocationField { get; set; }
        public string? Content { get; set; }
        public string? Location { get; set; }
        public bool IsNotApplicable { get; set; }
        public bool IsReported { get; set; }
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }

    public class ReviewChecklistSummaryDto
    {
        public Guid ReviewChecklistId { get; set; }
        public Guid ReviewId { get; set; }
        public string ReviewTitle { get; set; } = string.Empty;
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public double CompletionPercentage { get; set; }
        public int ItemCount { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }
    }

    public class ChecklistSectionDto
    {
        public string Section { get; set; } = string.Empty;
        public List<ChecklistItemResponseDto> Items { get; set; } = new();
    }

    public class ReviewChecklistDto
    {
        public Guid ReviewChecklistId { get; set; }
        public Guid ReviewId { get; set; }
        public string ReviewTitle { get; set; } = string.Empty;
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }
        public List<ChecklistSectionDto> Sections { get; set; } = new();
        public List<ChecklistItemResponseDto> Items { get; set; } = new();
    }

    public class ChecklistCompletionDto
    {
        public Guid ReviewChecklistId { get; set; }
        public double CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class GenerateReportRequest
    {
        public bool IncludeOnlyCompletedItems { get; set; }
    }
}
