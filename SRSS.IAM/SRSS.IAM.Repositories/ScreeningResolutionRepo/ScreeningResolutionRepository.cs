using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

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
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ScreeningResolutions
                .AsNoTracking()
                .Where(sr => sr.StudySelectionProcessId == studySelectionProcessId && sr.PaperId == paperId);

            if (phase.HasValue)
            {
                query = query.Where(sr => sr.Phase == phase.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<ScreeningResolution>> GetByProcessAsync(
            Guid studySelectionProcessId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ScreeningResolutions
                .AsNoTracking()
                .Where(sr => sr.StudySelectionProcessId == studySelectionProcessId);

            if (phase.HasValue)
            {
                query = query.Where(sr => sr.Phase == phase.Value);
            }

            return await query.ToListAsync(cancellationToken);
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

        public async Task<List<Guid>> GetResolvedPaperIdsByPhaseAsync(
            Guid studySelectionProcessId,
            ScreeningPhase phase,
            ScreeningDecisionType decision,
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningResolutions
                .AsNoTracking()
                .Where(sr => sr.StudySelectionProcessId == studySelectionProcessId
                          && sr.Phase == phase
                          && sr.FinalDecision == decision)
                .Select(sr => sr.PaperId)
                .ToListAsync(cancellationToken);
        }
    }
}
