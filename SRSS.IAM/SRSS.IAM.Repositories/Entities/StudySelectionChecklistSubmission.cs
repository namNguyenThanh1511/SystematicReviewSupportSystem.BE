using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents a reviewer's checklist submission for a specific paper.
    /// Bridge between ScreeningDecision and Checklist.
    /// </summary>
    public class StudySelectionChecklistSubmission : BaseEntity<Guid>
    {
        public Guid ScreeningDecisionId { get; set; }
        public Guid ChecklistTemplateId { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public int Version { get; set; }

        // Navigation Properties
        public ScreeningDecision ScreeningDecision { get; set; } = null!;
        public StudySelectionChecklistTemplate ChecklistTemplate { get; set; } = null!;
    }
}
