using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.StudySelectionChecklist
{
    // Template
    // Template
    public class StudySelectionChecklistTemplateSummaryDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
    }

    public class StudySelectionChecklistTemplateDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public List<StudySelectionChecklistTemplateSectionDto> Sections { get; set; } = new();
    }

    public class StudySelectionChecklistTemplateSectionDto
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }

        public List<StudySelectionChecklistTemplateItemDto> Items { get; set; } = new();
    }

    public class StudySelectionChecklistTemplateItemDto
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }

    }

    public class CreateStudySelectionChecklistTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CreateTemplateSectionRequest> Sections { get; set; } = new();
    }

    public class CreateTemplateSectionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public List<CreateTemplateItemRequest> Items { get; set; } = new();
    }

    public class CreateTemplateItemRequest
    {
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }

    }



    // Submission & Answer
    public class ChecklistSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid ScreeningDecisionId { get; set; }
        public Guid ChecklistTemplateId { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
    }

    public class CreateSubmissionRequest
    {
        public Guid ScreeningDecisionId { get; set; }
        public Guid ChecklistTemplateId { get; set; }
    }

    // Reviewer Specialized View
    public class PaperChecklistResponse
    {
        public Guid ChecklistTemplateId { get; set; }
        public List<StudySelectionChecklistTemplateSectionDto> Sections { get; set; } = new();
    }
}
