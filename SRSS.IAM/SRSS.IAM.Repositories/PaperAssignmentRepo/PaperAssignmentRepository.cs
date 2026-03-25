using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories;

namespace SRSS.IAM.Repositories.PaperAssignmentRepo
{
    public class PaperAssignmentRepository : GenericRepository<PaperAssignment, Guid, AppDbContext>, IPaperAssignmentRepository
    {
        public PaperAssignmentRepository(AppDbContext context) : base(context)
        {
        }
    }
}
