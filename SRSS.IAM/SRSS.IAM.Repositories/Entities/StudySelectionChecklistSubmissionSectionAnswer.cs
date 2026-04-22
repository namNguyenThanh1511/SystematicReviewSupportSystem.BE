using Shared.Entities.BaseEntity;
using System;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the checked state of a checklist section within a submission.
    /// Captures the snapshot of the section status at the time of submission.
    /// </summary>
    public class StudySelectionChecklistSubmissionSectionAnswer : BaseEntity<Guid>
    {
        public Guid SubmissionId { get; set; }
        public Guid SectionId { get; set; }
        public bool IsChecked { get; set; }

        // Navigation Properties
        public StudySelectionChecklistSubmission Submission { get; set; } = null!;
        public StudySelectionChecklistTemplateSection Section { get; set; } = null!;
    }
}
