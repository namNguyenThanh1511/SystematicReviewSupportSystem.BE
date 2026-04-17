using Shared.Entities.BaseEntity;
using System;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents an individual item (question) within a checklist template section.
    /// Used as a blueprint for creating checklist items in versions.
    /// </summary>
    public class StudySelectionChecklistTemplateItem : BaseEntity<Guid>
    {
        public Guid SectionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }

        // Navigation Properties
        public StudySelectionChecklistTemplateSection Section { get; set; } = null!;
    }
}
