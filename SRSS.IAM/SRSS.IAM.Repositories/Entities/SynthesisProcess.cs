using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.Entities
{
    public class SynthesisProcess : BaseEntity<Guid>
    {
        public Guid ReviewProcessId { get; set; }
        public SynthesisProcessStatus Status { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }

        public ReviewProcess ReviewProcess { get; set; } = null!;
        public ICollection<SynthesisTheme> Themes { get; set; } = new List<SynthesisTheme>();
        public ICollection<ResearchQuestionFinding> Findings { get; set; } = new List<ResearchQuestionFinding>();
    }
}
