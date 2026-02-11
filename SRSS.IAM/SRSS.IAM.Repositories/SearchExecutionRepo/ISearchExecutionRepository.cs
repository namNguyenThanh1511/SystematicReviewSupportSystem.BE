using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchExecutionRepo
{
    public interface ISearchExecutionRepository : IGenericRepository<SearchExecution, Guid, AppDbContext>
    {
    }
}
