using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.Paper
{
    public class AssignPapersRequest
    {
        [Required]
        public List<Guid> PaperIds { get; set; } = new();

        [Required]
        public List<Guid> MemberIds { get; set; } = new();
    }
}
