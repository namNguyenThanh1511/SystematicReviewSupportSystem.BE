using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.Entities
{
    public class SynthesisTheme : BaseEntity<Guid>
    {
        public Guid SynthesisProcessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ColorCode { get; set; }
        public Guid CreatedById { get; set; }

        public SynthesisProcess SynthesisProcess { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
        public ICollection<ThemeEvidence> Evidences { get; set; } = new List<ThemeEvidence>();
    }
}
