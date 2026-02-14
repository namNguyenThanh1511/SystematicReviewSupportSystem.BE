using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DeduplicationResultRepo
{
    public class DeduplicationResultRepository : GenericRepository<DeduplicationResult, Guid, AppDbContext>, IDeduplicationResultRepository
    {
        public DeduplicationResultRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<DeduplicationResult>> GetByIdentificationProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DeduplicationResults
                .AsNoTracking()
                .Where(dr => dr.IdentificationProcessId == identificationProcessId)
                .OrderBy(dr => dr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<DeduplicationResult?> GetByPaperAndProcessAsync(
            Guid paperId,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DeduplicationResults
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    dr => dr.PaperId == paperId && dr.IdentificationProcessId == identificationProcessId,
                    cancellationToken);
        }

        public async Task<int> CountDuplicatesByProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.DeduplicationResults
                .Where(dr => dr.IdentificationProcessId == identificationProcessId)
                .CountAsync(cancellationToken);
        }
    }
}
