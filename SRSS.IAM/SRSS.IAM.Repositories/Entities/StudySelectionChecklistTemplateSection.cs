using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents a section within a study selection checklist template.
    /// Used as a blueprint for creating checklist versions.
    /// </summary>
    public class StudySelectionChecklistTemplateSection : BaseEntity<Guid>
    {
        public Guid TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }


        // Navigation Properties
        public StudySelectionChecklistTemplate Template { get; set; } = null!;
        public ICollection<StudySelectionChecklistTemplateItem> Items { get; set; } = new List<StudySelectionChecklistTemplateItem>();
        public ICollection<StudySelectionChecklistSubmissionSectionAnswer> SectionAnswers { get; set; } = new List<StudySelectionChecklistSubmissionSectionAnswer>();
    }
}
