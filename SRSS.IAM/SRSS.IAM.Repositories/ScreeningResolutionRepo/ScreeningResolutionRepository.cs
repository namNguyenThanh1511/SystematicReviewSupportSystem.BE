using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ScreeningResolutionRepo
{
    public class ScreeningResolutionRepository : GenericRepository<ScreeningResolution, Guid, AppDbContext>, IScreeningResolutionRepository
    {
        public ScreeningResolutionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ScreeningResolution?> GetByProcessAndPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningResolutions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    sr => sr.StudySelectionProcessId == studySelectionProcessId && sr.PaperId == paperId,
                    cancellationToken);
        }

        public async Task<List<ScreeningResolution>> GetByProcessAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningResolutions
                .AsNoTracking()
                .Where(sr => sr.StudySelectionProcessId == studySelectionProcessId)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountByProcessAndDecisionAsync(
            Guid studySelectionProcessId,
            ScreeningDecisionType decision,
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningResolutions
                .Where(sr => sr.StudySelectionProcessId == studySelectionProcessId && sr.FinalDecision == decision)
                .CountAsync(cancellationToken);
        }
    }
}
