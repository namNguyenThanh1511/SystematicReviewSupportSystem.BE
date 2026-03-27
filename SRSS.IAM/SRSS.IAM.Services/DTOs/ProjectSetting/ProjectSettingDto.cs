using System;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.ProjectSetting
{
    public class ProjectSettingDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public int ReviewersPerPaperScreening { get; set; }
        public bool AutoResolveScreeningConflicts { get; set; }

        public int ReviewersPerPaperQuality { get; set; }
        public bool AutoResolveQualityConflicts { get; set; }
        
        public int ReviewersPerPaperExtraction { get; set; }
        public bool AutoResolveExtractionConflicts { get; set; }
        
        public DeduplicationStrictness DeduplicationStrictness { get; set; }
        public bool AutoResolveDuplicates { get; set; }
    }
}
