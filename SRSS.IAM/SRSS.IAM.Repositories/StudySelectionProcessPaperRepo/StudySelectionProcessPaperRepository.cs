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
    }
}
