using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.AuditLogRepo
{
    public class AuditLogRepository : GenericRepository<AuditLog, Guid, AppDbContext>, IAuditLogRepository
    {
        public AuditLogRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
