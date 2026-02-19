using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionProcessRepo
{
    public class StudySelectionProcessRepository : GenericRepository<StudySelectionProcess, Guid, AppDbContext>, IStudySelectionProcessRepository
    {
        public StudySelectionProcessRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<StudySelectionProcess?> GetByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionProcesses
                .AsNoTracking()
                .Include(ssp => ssp.ScreeningDecisions)
                .Include(ssp => ssp.ScreeningResolutions)
                .FirstOrDefaultAsync(ssp => ssp.ReviewProcessId == reviewProcessId, cancellationToken);
        }

        public async Task<StudySelectionProcess?> GetActiveByReviewProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    ssp => ssp.ReviewProcessId == reviewProcessId 
                        && ssp.Status == SelectionProcessStatus.InProgress,
                    cancellationToken);
        }

        public async Task<bool> HasActiveProcessAsync(
            Guid reviewProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionProcesses
                .AnyAsync(
                    ssp => ssp.ReviewProcessId == reviewProcessId 
                        && ssp.Status == SelectionProcessStatus.InProgress,
                    cancellationToken);
        }

        public Task<StudySelectionProcess?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.StudySelectionProcesses
                .AsNoTracking()
                .Include(ssp => ssp.ScreeningDecisions)
                .Include(ssp => ssp.ScreeningResolutions)
                .FirstOrDefaultAsync(ssp => ssp.Id == id, cancellationToken);
        }
    }
}
