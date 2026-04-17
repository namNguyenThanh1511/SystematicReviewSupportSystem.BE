using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.StudySelectionChecklist;

namespace SRSS.IAM.Services.Mappers
{
    public static class StudySelectionChecklistMappingExtension
    {
        public static StudySelectionChecklistTemplateDto MapToDto(this StudySelectionChecklistTemplate entity)
        {
            return new StudySelectionChecklistTemplateDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                Name = entity.Name,
                Description = entity.Description,
                Version = entity.Version,
                IsActive = entity.IsActive,
                Sections = entity.Sections.Select(s => s.MapToDto()).ToList()
            };
        }

        public static StudySelectionChecklistTemplateSummaryDto MapToSummaryDto(this StudySelectionChecklistTemplate entity)
        {
            return new StudySelectionChecklistTemplateSummaryDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                Name = entity.Name,
                Description = entity.Description,
                Version = entity.Version,
                IsActive = entity.IsActive
            };
        }

        public static StudySelectionChecklistTemplateSectionDto MapToDto(this StudySelectionChecklistTemplateSection entity)
        {
            return new StudySelectionChecklistTemplateSectionDto
            {
                Id = entity.Id,
                TemplateId = entity.TemplateId,
                Title = entity.Title,
                Description = entity.Description,
                Order = entity.Order,

                Items = entity.Items.Select(i => i.MapToDto()).ToList()
            };
        }

        public static StudySelectionChecklistTemplateItemDto MapToDto(this StudySelectionChecklistTemplateItem entity)
        {
            return new StudySelectionChecklistTemplateItemDto
            {
                Id = entity.Id,
                SectionId = entity.SectionId,
                Text = entity.Text,
                Order = entity.Order,

            };
        }

        public static ChecklistSubmissionDto MapToDto(this StudySelectionChecklistSubmission entity)
        {
            return new ChecklistSubmissionDto
            {
                Id = entity.Id,
                ScreeningDecisionId = entity.ScreeningDecisionId,
                ChecklistTemplateId = entity.ChecklistTemplateId,
                SubmittedAt = entity.SubmittedAt
            };
        }

    }
}
