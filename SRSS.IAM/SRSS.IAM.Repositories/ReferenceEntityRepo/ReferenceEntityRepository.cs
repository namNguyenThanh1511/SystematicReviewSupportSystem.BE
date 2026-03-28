using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ReferenceEntityRepo
{
    public class ReferenceEntityRepository : GenericRepository<ReferenceEntity, Guid, AppDbContext>, IReferenceEntityRepository
    {
        public ReferenceEntityRepository(AppDbContext context) : base(context)
        {
        }
    }
}
