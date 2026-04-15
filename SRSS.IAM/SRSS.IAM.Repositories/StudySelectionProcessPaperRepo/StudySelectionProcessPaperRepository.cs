using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.StudySelectionProcessPaperRepo
{
    public class StudySelectionProcessPaperRepository : GenericRepository<StudySelectionProcessPaper, Guid, AppDbContext>, IStudySelectionProcessPaperRepository
    {
        public StudySelectionProcessPaperRepository(AppDbContext context) : base(context)
        {
        }

        public async Task DeleteByProcessAsync(Guid processId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.StudySelectionProcessPapers
                .Where(p => p.StudySelectionProcessId == processId)
                .ToListAsync(cancellationToken);

            _context.StudySelectionProcessPapers.RemoveRange(existing);
        }

        public async Task<(List<StudySelectionProcessPaper> Items, int TotalCount)> GetWithPaperByProcessAsync(
            Guid processId, 
            string? search = null, 
            int pageNumber = 1, 
            int pageSize = 10, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.StudySelectionProcessPapers
                .AsNoTracking()
                .Include(p => p.Paper)
                .Where(p => p.StudySelectionProcessId == processId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.Paper.Title.ToLower().Contains(searchLower) ||
                    (p.Paper.Authors != null && p.Paper.Authors.ToLower().Contains(searchLower)) ||
                    (p.Paper.DOI != null && p.Paper.DOI.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(p => p.Paper.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        public async Task<List<StudySelectionProcessPaper>> GetWithPaperByProcessAsync(
            Guid processId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.StudySelectionProcessPapers
                .AsNoTracking()
                .Include(p => p.Paper)
                .Where(p => p.StudySelectionProcessId == processId)
                .ToListAsync(cancellationToken);
        }
    }
}
