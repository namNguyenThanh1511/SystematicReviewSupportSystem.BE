using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.ScreeningDecisionRepo
{
    public class ScreeningDecisionRepository : GenericRepository<ScreeningDecision, Guid, AppDbContext>, IScreeningDecisionRepository
    {
        public ScreeningDecisionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<ScreeningDecision>> GetByProcessAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningDecisions
                .AsNoTracking()
                .Include(sd => sd.Paper)
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId)
                .OrderBy(sd => sd.DecidedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ScreeningDecision>> GetByPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ScreeningDecisions
                .AsNoTracking()
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId && sd.PaperId == paperId);

            if (phase.HasValue)
            {
                query = query.Where(sd => sd.Phase == phase.Value);
            }

            return await query.OrderBy(sd => sd.DecidedAt).ToListAsync(cancellationToken);
        }

        public async Task<ScreeningDecision?> GetByReviewerAndPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ScreeningDecisions
                .AsNoTracking()
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId
                    && sd.PaperId == paperId
                    && sd.ReviewerId == reviewerId);

            if (phase.HasValue)
            {
                query = query.Where(sd => sd.Phase == phase.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> HasDecisionAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            Guid reviewerId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ScreeningDecisions
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId
                    && sd.PaperId == paperId
                    && sd.ReviewerId == reviewerId);

            if (phase.HasValue)
            {
                query = query.Where(sd => sd.Phase == phase.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<List<Guid>> GetPapersWithConflictsAsync(
            Guid studySelectionProcessId,
            ScreeningPhase? phase = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ScreeningDecisions
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId);

            if (phase.HasValue)
            {
                query = query.Where(sd => sd.Phase == phase.Value);
            }

            // Papers with conflicting decisions (Include AND Exclude from different reviewers)
            var papersWithConflicts = await query
                .GroupBy(sd => sd.PaperId)
                .Where(g => g.Select(d => d.Decision).Distinct().Count() > 1) // Has both Include and Exclude
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            return papersWithConflicts;
        }

        public Task<int> CountScreenedPapersAsync(Guid studySelectionProcessId, CancellationToken cancellationToken = default)
        {
            return _context.ScreeningDecisions
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId)
                .Select(sd => sd.PaperId)
                .Distinct()
                .CountAsync(cancellationToken);
        }

    }
}
