using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.IdentificationProcessRepo
{
    public interface IIdentificationProcessRepository : IGenericRepository<IdentificationProcess, Guid, AppDbContext>
    {
        Task<IdentificationProcess> GetByIdWithProjectAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
