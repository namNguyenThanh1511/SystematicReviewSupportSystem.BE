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

        public async Task<List<PrismaReport>> GetReportsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.PrismaReports
                .AsNoTracking()
                .Include(pr => pr.FlowRecords.OrderBy(fr => fr.DisplayOrder))
                .Where(pr => pr.ProjectId == projectId)
                .OrderByDescending(pr => pr.GeneratedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<PrismaReport?> GetLatestReportByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.PrismaReports
                .AsNoTracking()
                .Include(pr => pr.FlowRecords.OrderBy(fr => fr.DisplayOrder))
                .Where(pr => pr.ProjectId == projectId)
                .OrderByDescending(pr => pr.GeneratedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
