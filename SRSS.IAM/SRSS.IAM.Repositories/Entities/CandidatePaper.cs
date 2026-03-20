using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class CandidatePaper : BaseEntity<Guid>
    {
        public Guid ReviewProcessId { get; set; }
        public Guid? OriginPaperId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? PublicationYear { get; set; }
        public string? DOI { get; set; }
        public string? RawReference { get; set; }
        public string? NormalizedReference { get; set; }
        
        public CandidateStatus Status { get; set; } = CandidateStatus.Detected;

        // Navigation Properties
        public ReviewProcess ReviewProcess { get; set; } = null!;
        public Paper OriginPaper { get; set; } = null!;
    }
}
