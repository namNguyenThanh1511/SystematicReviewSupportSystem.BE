using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
    public class ProjectPicoc : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public string? Population { get; set; }
        public string? Intervention { get; set; }
        public string? Comparator { get; set; }
        public string? Outcome { get; set; }
        public string? Context { get; set; }

        // Navigation properties
        public SystematicReviewProject Project { get; set; } = null!;
    }
}
