using Shared.Entities.BaseEntity;
using System;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the checked state of an individual checklist item within a submission.
    /// Stores the reviewer's response to a specific checklist item.
    /// </summary>
    public class StudySelectionChecklistSubmissionItemAnswer : BaseEntity<Guid>
    {
        public Guid SubmissionId { get; set; }
        public Guid ItemId { get; set; }
        public bool IsChecked { get; set; }

        // Navigation Properties
        public StudySelectionChecklistSubmission Submission { get; set; } = null!;
        public StudySelectionChecklistTemplateItem Item { get; set; } = null!;
    }
}
