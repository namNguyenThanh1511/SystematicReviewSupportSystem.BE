using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the logical checklist definition for a project.
    /// Only one template per project.
    /// </summary>
    public class StudySelectionChecklistTemplate : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Version { get; set; } = 1;

        // Navigation Properties
        public SystematicReviewProject Project { get; set; } = null!;
        public ICollection<StudySelectionChecklistTemplateSection> Sections { get; set; } = new List<StudySelectionChecklistTemplateSection>();
        public ICollection<StudySelectionChecklistSubmission> Submissions { get; set; } = new List<StudySelectionChecklistSubmission>();
    }
}
