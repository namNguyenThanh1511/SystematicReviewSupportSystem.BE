using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.Entities
{
    public class ReferenceEntity : BaseEntity<Guid>
    {
        public string? Title { get; set; }
        public string? Authors { get; set; }
        public string? DOI { get; set; }
        public string? Url { get; set; }

        public ReferenceType Type { get; set; }

        public string? RawReference { get; set; }

        // Navigation properties
        public ICollection<PaperCitation> IncomingCitations { get; set; } = new List<PaperCitation>();
    }
}
