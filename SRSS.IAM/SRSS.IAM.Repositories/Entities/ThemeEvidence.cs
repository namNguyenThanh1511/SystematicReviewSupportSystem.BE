using Shared.Entities.BaseEntity;
using System;

namespace SRSS.IAM.Repositories.Entities
{
    public class ThemeEvidence : BaseEntity<Guid>
    {
        public Guid ThemeId { get; set; }
        public Guid ExtractedDataValueId { get; set; }
        public string? Notes { get; set; }
        public Guid CreatedById { get; set; }

        public SynthesisTheme Theme { get; set; } = null!;
        public ExtractedDataValue ExtractedDataValue { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
    }
}
