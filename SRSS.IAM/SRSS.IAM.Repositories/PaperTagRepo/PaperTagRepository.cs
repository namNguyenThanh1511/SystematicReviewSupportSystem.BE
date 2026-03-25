using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperTagRepo
{
    public class PaperTagRepository : GenericRepository<PaperTag, Guid, AppDbContext>, IPaperTagRepository
    {
        public PaperTagRepository(AppDbContext context) : base(context) { }

        public async Task<List<PaperTag>> GetTagsByPaperAsync(Guid paperId, CancellationToken cancellationToken = default)
        {
            return await _context.PaperTags
                .AsNoTracking()
                .Where(t => t.PaperId == paperId)
                .OrderBy(t => t.Phase)
                .ThenBy(t => t.Label)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PaperTag>> GetTagsByPaperAndPhaseAsync(Guid paperId, ProcessPhase phase, CancellationToken cancellationToken = default)
        {
            return await _context.PaperTags
                .AsNoTracking()
                .Where(t => t.PaperId == paperId && t.Phase == phase)
                .OrderBy(t => t.Label)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaperTag?> GetExistingTagAsync(Guid paperId, Guid userId, ProcessPhase phase, string label, CancellationToken cancellationToken = default)
        {
            return await _context.PaperTags
                .Where(t => t.PaperId == paperId && t.UserId == userId && t.Phase == phase && t.Label == label)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
