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
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public Guid ReviewerId { get; set; }
        public ScreeningPhase Phase { get; set; }
        public Guid ChecklistTemplateId { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public List<SubmissionSectionAnswerDto> SectionAnswers { get; set; } = new();
        public List<SubmissionItemAnswerDto> ItemAnswers { get; set; } = new();
    }

    public class CreateSubmissionRequest
    {
        public Guid StudySelectionProcessId { get; set; }
        public Guid PaperId { get; set; }
        public Guid ReviewerId { get; set; }
        public ScreeningPhase Phase { get; set; }
        public Guid ChecklistTemplateId { get; set; }
        public List<SubmissionSectionAnswerDto> SectionAnswers { get; set; } = new();
        public List<SubmissionItemAnswerDto> ItemAnswers { get; set; } = new();
    }

    public class SubmissionSectionAnswerDto
    {
        public Guid SectionId { get; set; }
        public bool IsChecked { get; set; }
    }

    public class SubmissionItemAnswerDto
    {
        public Guid ItemId { get; set; }
        public bool IsChecked { get; set; }
    }

    // Reviewer Specialized View
    public class PaperChecklistResponse
    {
        public Guid ChecklistTemplateId { get; set; }
        public List<StudySelectionChecklistTemplateSectionDto> Sections { get; set; } = new();
    }

    public class ChecklistReviewDto
    {
        public Guid? SubmissionId { get; set; }
        public bool IsFromTemplate { get; set; }
        public Guid ChecklistTemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public List<ChecklistReviewSectionDto> Sections { get; set; } = new();
    }

    public class ChecklistReviewSectionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
        public List<ChecklistReviewItemDto> Items { get; set; } = new();
    }

    public class ChecklistReviewItemDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}
