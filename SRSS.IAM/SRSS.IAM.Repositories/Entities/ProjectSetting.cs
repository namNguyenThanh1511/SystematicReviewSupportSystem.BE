using Shared.Entities.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SRSS.IAM.Repositories.Entities
{
    public class ProjectSetting : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }


        // Screening settings
        public int ReviewersPerPaperScreening { get; set; } = 2; // Default 2
        public bool AutoResolveScreeningConflicts { get; set; } = false;

        // Quality assessment settings
        public int ReviewersPerPaperQuality { get; set; } = 2; // Default 2
        public bool AutoResolveQualityConflicts { get; set; } = false;

        // Data extraction settings
        public int ReviewersPerPaperExtraction { get; set; } = 2; // Default 2
        public bool AutoResolveExtractionConflicts { get; set; } = false;

        // Identification settings
        public DeduplicationStrictness DeduplicationStrictness { get; set; } = DeduplicationStrictness.ExactMatch;
        public bool AutoResolveDuplicates { get; set; } = true;

        // Navigation property
        public SystematicReviewProject Project { get; set; }
    }
}
