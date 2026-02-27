using SRSS.IAM.Services.DTOs.ProjectMemberInvitation;

namespace SRSS.IAM.Services.ProjectMemberInvitationService
{
    public interface IProjectInvitationService
    {
        Task CreateInvitationsAsync(Guid projectId, Guid inviterUserId, CreateProjectInvitationRequest request);
    }
}
