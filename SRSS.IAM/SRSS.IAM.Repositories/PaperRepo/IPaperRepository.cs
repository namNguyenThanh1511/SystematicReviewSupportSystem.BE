using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperRepo
{
    public interface IPaperRepository : IGenericRepository<Paper, Guid, AppDbContext>
    {
        Task<Paper?> GetByDoiAndProjectAsync(string doi,Guid projectId , CancellationToken cancellationToken = default);
    }
}

