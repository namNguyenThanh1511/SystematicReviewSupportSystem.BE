using System.ComponentModel.DataAnnotations;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.Paper
{
    public class AssignPapersRequest
    {
        [Required]
        public List<Guid> PaperIds { get; set; } = new();

        [Required]
        public List<Guid> MemberIds { get; set; } = new();

        [Required]
        public Guid StudySelectionProcessId { get; set; }

        public ScreeningPhase Phase { get; set; } = ScreeningPhase.TitleAbstract;
    }
}
