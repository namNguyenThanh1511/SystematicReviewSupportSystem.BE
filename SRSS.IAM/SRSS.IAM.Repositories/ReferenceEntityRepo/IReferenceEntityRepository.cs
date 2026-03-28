using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.ReferenceEntityRepo
{
    public interface IReferenceEntityRepository : IGenericRepository<ReferenceEntity, Guid, AppDbContext>
    {
    }
}
