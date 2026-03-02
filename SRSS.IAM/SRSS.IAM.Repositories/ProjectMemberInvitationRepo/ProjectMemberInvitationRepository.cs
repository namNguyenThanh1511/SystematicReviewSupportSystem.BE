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

        public async Task<IEnumerable<ProjectMemberInvitation>> GetByProjectIdAsync(Guid projectId, ProjectMemberInvitationStatus? status = null)
        {
            var query = _context.ProjectMemberInvitations
                .Include(i => i.InvitedUser)
                .Include(i => i.InvitedByUser)
                .Where(i => i.ProjectId == projectId);

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ProjectMemberInvitation>> GetByInvitedUserIdAsync(Guid userId, bool includeExpired = false)
        {
            var query = _context.ProjectMemberInvitations
                .Include(i => i.Project)
                .Include(i => i.InvitedByUser)
                .Where(i => i.InvitedUserId == userId);

            if (!includeExpired)
            {
                query = query.Where(i => i.Status == ProjectMemberInvitationStatus.Pending &&
                                       (i.ExpiredAt == null || i.ExpiredAt > DateTimeOffset.UtcNow));
            }

            return await query.ToListAsync();
        }

        public async Task<ProjectMemberInvitation?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.ProjectMemberInvitations
                .Include(i => i.Project)
                .Include(i => i.InvitedUser)
                .Include(i => i.InvitedByUser)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}
