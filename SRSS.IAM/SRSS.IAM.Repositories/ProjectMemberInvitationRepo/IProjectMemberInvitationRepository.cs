using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ProjectMemberInvitationRepo
{
    public interface IProjectMemberInvitationRepository : IGenericRepository<ProjectMemberInvitation, Guid, AppDbContext>
    {
        Task AddRangeAsync(IEnumerable<ProjectMemberInvitation> invitations);
        Task<IEnumerable<ProjectMemberInvitation>> GetByProjectIdAsync(Guid projectId, ProjectMemberInvitationStatus? status = null);
        Task<ProjectMemberInvitation?> GetByIdWithDetailsAsync(Guid id);
    }
}
