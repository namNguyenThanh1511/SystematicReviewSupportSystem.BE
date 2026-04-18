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
                StudySelectionProcessId = entity.StudySelectionProcessId,
                PaperId = entity.PaperId,
                ReviewerId = entity.ReviewerId,
                Phase = entity.Phase,
                ChecklistTemplateId = entity.ChecklistTemplateId,
                SubmittedAt = entity.SubmittedAt,
                SectionAnswers = entity.SectionAnswers?.Select(sa => new SubmissionSectionAnswerDto
                {
                    SectionId = sa.SectionId,
                    IsChecked = sa.IsChecked
                }).ToList() ?? new(),
                ItemAnswers = entity.ItemAnswers?.Select(ia => new SubmissionItemAnswerDto
                {
                    ItemId = ia.ItemId,
                    IsChecked = ia.IsChecked
                }).ToList() ?? new()
            };
        }

        public static ChecklistReviewDto MapToReviewDto(this StudySelectionChecklistTemplate template, StudySelectionChecklistSubmission? submission = null)
        {
            var sectionAnswers = submission?.SectionAnswers?
                .GroupBy(a => a.SectionId)
                .ToDictionary(g => g.Key, g => g.First().IsChecked) ?? new();

            var itemAnswers = submission?.ItemAnswers?
                .GroupBy(a => a.ItemId)
                .ToDictionary(g => g.Key, g => g.First().IsChecked) ?? new();

            return new ChecklistReviewDto
            {
                SubmissionId = submission?.Id,
                IsFromTemplate = submission == null,
                ChecklistTemplateId = template.Id,
                Name = template.Name,
                Description = template.Description,
                SubmittedAt = submission?.SubmittedAt,
                Sections = template.Sections.OrderBy(s => s.Order).Select(s => new ChecklistReviewSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    IsChecked = sectionAnswers.GetValueOrDefault(s.Id, false),
                    Items = s.Items.OrderBy(i => i.Order).Select(i => new ChecklistReviewItemDto
                    {
                        Id = i.Id,
                        Text = i.Text,
                        IsChecked = itemAnswers.GetValueOrDefault(i.Id, false)
                    }).ToList()
                }).ToList()
            };
        }
    }
}
