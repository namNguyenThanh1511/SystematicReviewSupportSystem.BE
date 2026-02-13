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

        public async Task<IdentificationProcess> GetByIdWithProjectAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.IdentificationProcesses
                .Include(ip => ip.ReviewProcess)
                .FirstOrDefaultAsync(ip => ip.Id == id, cancellationToken);
        }
    }
}
