using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories;

namespace SRSS.IAM.Repositories.PaperAssignmentRepo
{
    public interface IPaperAssignmentRepository : IGenericRepository<PaperAssignment, Guid, AppDbContext>
    {
    }
}
