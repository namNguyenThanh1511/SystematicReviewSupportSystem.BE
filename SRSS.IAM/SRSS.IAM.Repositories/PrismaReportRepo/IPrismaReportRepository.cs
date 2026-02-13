using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PrismaReportRepo
{
    public interface IPrismaReportRepository : IGenericRepository<PrismaReport, Guid, AppDbContext>
    {
        Task<List<PrismaReport>> GetReportsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<PrismaReport>> GetReportsByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PrismaReport?> GetLatestReportByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    }
}
