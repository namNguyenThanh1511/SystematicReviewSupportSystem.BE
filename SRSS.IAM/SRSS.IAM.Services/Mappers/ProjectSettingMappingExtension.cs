using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.ProjectSetting;

namespace SRSS.IAM.Services.Mappers
{
    public static class ProjectSettingMappingExtension
    {
        public static ProjectSettingDto ToResponse(this ProjectSetting entity) => new()
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            ReviewersPerPaperScreening = entity.ReviewersPerPaperScreening,
            ReviewersPerPaperQuality = entity.ReviewersPerPaperQuality,
            ReviewersPerPaperExtraction = entity.ReviewersPerPaperExtraction,
            AutoResolveScreeningConflicts = entity.AutoResolveScreeningConflicts,
            AutoResolveQualityConflicts = entity.AutoResolveQualityConflicts,
            AutoResolveExtractionConflicts = entity.AutoResolveExtractionConflicts,
            DeduplicationStrictness = entity.DeduplicationStrictness,
            AutoResolveDuplicates = entity.AutoResolveDuplicates
        };

        public static void ApplyTo(this UpdateProjectSettingRequest r, ProjectSetting entity)
        {
            entity.ReviewersPerPaperScreening = r.ReviewersPerPaperScreening;
            entity.ReviewersPerPaperQuality = r.ReviewersPerPaperQuality;
            entity.ReviewersPerPaperExtraction = r.ReviewersPerPaperExtraction;
            entity.AutoResolveScreeningConflicts = r.AutoResolveScreeningConflicts;
            entity.AutoResolveQualityConflicts = r.AutoResolveQualityConflicts;
            entity.AutoResolveExtractionConflicts = r.AutoResolveExtractionConflicts;
            entity.DeduplicationStrictness = r.DeduplicationStrictness;
            entity.AutoResolveDuplicates = r.AutoResolveDuplicates;
        }
    }
}
