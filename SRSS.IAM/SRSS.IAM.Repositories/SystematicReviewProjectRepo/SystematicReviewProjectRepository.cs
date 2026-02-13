using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SystematicReviewProjectRepo
{
    public class SystematicReviewProjectRepository : GenericRepository<SystematicReviewProject, Guid, AppDbContext>, ISystematicReviewProjectRepository
    {
        public SystematicReviewProjectRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<SystematicReviewProject?> GetByIdWithProcessesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.SystematicReviewProjects
                .Include(p => p.ReviewProcesses)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<SystematicReviewProject>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SystematicReviewProjects
                .Where(p => p.Status == ProjectStatus.Active)
                .Include(p => p.ReviewProcesses)
                .ToListAsync(cancellationToken);
        }

        public IQueryable<SystematicReviewProject> GetQueryable()
        {
            return _context.SystematicReviewProjects.AsQueryable();
        }
    }
}
