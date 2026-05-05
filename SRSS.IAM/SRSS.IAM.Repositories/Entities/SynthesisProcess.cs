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
        public ICollection<DataSynthesisStrategy> SynthesisStrategies { get; set; } = new List<DataSynthesisStrategy>();
        
        // Domain Method
        public void Start()
		{
			if (Status != SynthesisProcessStatus.NotStarted)
			{
				throw new InvalidOperationException($"Cannot start synthesis process from {Status} status.");
			}

			Status = SynthesisProcessStatus.InProgress;
			StartedAt = DateTimeOffset.UtcNow;
			ModifiedAt = DateTimeOffset.UtcNow;
		}

		public void Complete()
		{
			if (Status != SynthesisProcessStatus.InProgress && Status != SynthesisProcessStatus.Reopened)
			{
				throw new InvalidOperationException($"Cannot complete synthesis process from {Status} status.");
			}

			Status = SynthesisProcessStatus.Completed;
			CompletedAt = DateTimeOffset.UtcNow;
			ModifiedAt = DateTimeOffset.UtcNow;
		}

		public void Reopen()
		{
			if (Status != SynthesisProcessStatus.Completed)
			{
				throw new InvalidOperationException($"Cannot reopen synthesis process from {Status} status.");
			}

			Status = SynthesisProcessStatus.Reopened;
			CompletedAt = null;
			ModifiedAt = DateTimeOffset.UtcNow;
		}
    }
}
