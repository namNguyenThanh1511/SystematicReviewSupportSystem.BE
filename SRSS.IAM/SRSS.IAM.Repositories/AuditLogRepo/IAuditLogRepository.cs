using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.AuditLogRepo
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog, Guid, AppDbContext>
    {
    }
}
