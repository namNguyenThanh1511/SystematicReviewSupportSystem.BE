using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PrismaReportRepo
{
    public interface IPrismaReportRepository : IGenericRepository<PrismaReport, Guid, AppDbContext>
    {
        Task<List<PrismaReport>> GetReportsByReviewProcessAsync(Guid reviewProcessId, CancellationToken cancellationToken = default);
        Task<List<PrismaReport>> GetReportsByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PrismaReport?> GetLatestReportByReviewProcessAsync(Guid reviewProcessId, CancellationToken cancellationToken = default);
    }
}
