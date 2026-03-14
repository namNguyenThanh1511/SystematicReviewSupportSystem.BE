using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.TitleAbstractScreeningRepo
{
    public class TitleAbstractScreeningRepository : GenericRepository<TitleAbstractScreening, Guid, AppDbContext>, ITitleAbstractScreeningRepository
    {
        public TitleAbstractScreeningRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<TitleAbstractScreening?> GetByProcessIdAsync(
            Guid studySelectionProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.TitleAbstractScreenings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    ta => ta.StudySelectionProcessId == studySelectionProcessId,
                    cancellationToken);
        }
    }
}
