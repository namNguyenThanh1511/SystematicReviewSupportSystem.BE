using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ReviewProcessRepo
{
    public class ReviewProcessRepository : GenericRepository<ReviewProcess, Guid, AppDbContext>, IReviewProcessRepository
    {
        public ReviewProcessRepository(AppDbContext context) : base(context)
        {
        }

        public Task<ReviewProcess?> GetByIdWithProcessesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.ReviewProcesses
                .Include(rp => rp.IdentificationProcess)
                .Include(rp => rp.StudySelectionProcess)
                .FirstOrDefaultAsync(rp => rp.Id == id, cancellationToken);

        }

        public async Task<ReviewProcess?> GetByIdWithProjectAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ReviewProcesses
                .Include(rp => rp.Project)
                    .ThenInclude(p => p.ReviewProcesses)
                .FirstOrDefaultAsync(rp => rp.Id == id, cancellationToken);
        }



        public async Task<IEnumerable<ReviewProcess>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.ReviewProcesses
                .Where(rp => rp.ProjectId == projectId)
                .ToListAsync(cancellationToken);
        }

        public async Task<ReviewProcess?> GetInProgressProcessForProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.ReviewProcesses
                .FirstOrDefaultAsync(rp => rp.ProjectId == projectId && rp.Status == ProcessStatus.InProgress, cancellationToken);
        }


    }
}
