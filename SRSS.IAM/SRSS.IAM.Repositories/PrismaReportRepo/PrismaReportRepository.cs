using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PrismaReportRepo
{
    public class PrismaReportRepository : GenericRepository<PrismaReport, Guid, AppDbContext>, IPrismaReportRepository
    {
        public PrismaReportRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<PrismaReport>> GetReportsByReviewProcessAsync(Guid reviewProcessId, CancellationToken cancellationToken = default)
        {
            return await _context.PrismaReports
                .AsNoTracking()
                .Include(pr => pr.FlowRecords.OrderBy(fr => fr.DisplayOrder))
                .Where(pr => pr.ReviewProcessId == reviewProcessId)
                .OrderByDescending(pr => pr.GeneratedAt)
                .ToListAsync(cancellationToken);
        }



        public async Task<PrismaReport?> GetLatestReportByReviewProcessAsync(Guid reviewProcessId, CancellationToken cancellationToken = default)
        {
            return await _context.PrismaReports
                .AsNoTracking()
                .Include(pr => pr.FlowRecords.OrderBy(fr => fr.DisplayOrder))
                .Where(pr => pr.ReviewProcessId == reviewProcessId)
                .OrderByDescending(pr => pr.GeneratedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<List<PrismaReport>> GetReportsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.PrismaReports
                .AsNoTracking()
                .Include(pr => pr.FlowRecords.OrderBy(fr => fr.DisplayOrder))
                .Where(pr => pr.Id == id)
                .OrderByDescending(pr => pr.GeneratedAt)
                .ToListAsync(cancellationToken);

        }
    }
}
