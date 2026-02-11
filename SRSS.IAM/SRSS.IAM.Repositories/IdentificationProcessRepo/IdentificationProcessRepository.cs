using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.IdentificationProcessRepo
{
    public class IdentificationProcessRepository : GenericRepository<IdentificationProcess, Guid, AppDbContext>, IIdentificationProcessRepository
    {
        public IdentificationProcessRepository(AppDbContext context) : base(context)
        {
        }
    }
}
