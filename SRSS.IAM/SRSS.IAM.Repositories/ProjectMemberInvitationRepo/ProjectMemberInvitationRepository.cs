using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ProjectMemberInvitationRepo
{
    public class ProjectMemberInvitationRepository : GenericRepository<ProjectMemberInvitation, Guid, AppDbContext>, IProjectMemberInvitationRepository
    {
        public ProjectMemberInvitationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task AddRangeAsync(IEnumerable<ProjectMemberInvitation> invitations)
        {
            await _context.ProjectMemberInvitations.AddRangeAsync(invitations);
        }
    }
}
