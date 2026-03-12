using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.IdentificationProcessPaperRepo
{
    public class IdentificationProcessPaperRepository : GenericRepository<IdentificationProcessPaper, Guid, AppDbContext>, IIdentificationProcessPaperRepository
    {
        public IdentificationProcessPaperRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Guid>> GetIncludedPaperIdsByProcessAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.IdentificationProcessPapers
                .AsNoTracking()
                .Where(ipp => ipp.IdentificationProcessId == identificationProcessId && ipp.IncludedAfterDedup)
                .Select(ipp => ipp.PaperId)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> SnapshotExistsAsync(
            Guid identificationProcessId,
            CancellationToken cancellationToken = default)
        {
            return await _context.IdentificationProcessPapers
                .AsNoTracking()
                .AnyAsync(ipp => ipp.IdentificationProcessId == identificationProcessId, cancellationToken);
        }
    }
}
