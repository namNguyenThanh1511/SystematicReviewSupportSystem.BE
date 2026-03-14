using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System.Diagnostics;

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
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningDecisions
                .AsNoTracking()
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId && sd.PaperId == paperId)
                .OrderBy(sd => sd.DecidedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<ScreeningDecision?> GetByReviewerAndPaperAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningDecisions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    sd => sd.StudySelectionProcessId == studySelectionProcessId 
                        && sd.PaperId == paperId 
                        && sd.ReviewerId == reviewerId,
                    cancellationToken);
        }

        public async Task<bool> HasDecisionAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            Guid reviewerId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ScreeningDecisions
                .AnyAsync(
                    sd => sd.StudySelectionProcessId == studySelectionProcessId 
                        && sd.PaperId == paperId 
                        && sd.ReviewerId == reviewerId,
                    cancellationToken);
        }

        public async Task<List<Guid>> GetPapersWithConflictsAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            // Papers with conflicting decisions (Include AND Exclude from different reviewers)
            var papersWithConflicts = await _context.ScreeningDecisions
                .Where(sd => sd.StudySelectionProcessId == studySelectionProcessId)
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
