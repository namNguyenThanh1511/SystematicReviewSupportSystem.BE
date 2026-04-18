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

        public Task<StudySelectionProcess?> GetPhaseStatusAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.StudySelectionProcesses
                .AsNoTracking()
                .Include(ssp => ssp.TitleAbstractScreening)
                .Include(ssp => ssp.FullTextScreening)
                .FirstOrDefaultAsync(ssp => ssp.Id == id, cancellationToken);
        }

        public async Task<StudySelectionProcess?> GetForAiEvaluationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionProcesses
                .AsNoTracking()
                .Include(ssp => ssp.ReviewProcess)
                    .ThenInclude(rp => rp.Project)
                        .ThenInclude(p => p.ResearchQuestions)
                            .ThenInclude(rq => rq.PicocElements)
                .Include(ssp => ssp.ReviewProcess)
                    .ThenInclude(rp => rp.Protocol)
                .Include(ssp => ssp.ReviewProcess)
                    .ThenInclude(rp => rp.Protocol)
                        .ThenInclude(p => p.SelectionCriterias)
                            .ThenInclude(sc => sc.InclusionCriteria)
                .Include(ssp => ssp.ReviewProcess)
                    .ThenInclude(rp => rp.Protocol)
                        .ThenInclude(p => p.SelectionCriterias)
                            .ThenInclude(sc => sc.ExclusionCriteria)
                .FirstOrDefaultAsync(ssp => ssp.Id == id, cancellationToken);
        }
    }
}
