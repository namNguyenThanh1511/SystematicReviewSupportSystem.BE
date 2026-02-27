using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.DTOs.ProjectMemberInvitation
{
    public class CreateProjectInvitationRequest
    {
        public List<Guid> UserIds { get; set; } = new();
        public ProjectRole Role { get; set; }
        public DateTimeOffset? ExpiredAt { get; set; }
    }
}
