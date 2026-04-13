using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;
using System;

namespace SRSS.IAM.Repositories.Entities
{
    public class ResearchQuestionFinding : BaseEntity<Guid>
    {
        public Guid SynthesisProcessId { get; set; }
        public Guid ResearchQuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public FindingStatus Status { get; set; }
        public Guid AuthorId { get; set; }

        public SynthesisProcess SynthesisProcess { get; set; } = null!;
        public ResearchQuestion ResearchQuestion { get; set; } = null!;
        public User Author { get; set; } = null!;
    }
}
