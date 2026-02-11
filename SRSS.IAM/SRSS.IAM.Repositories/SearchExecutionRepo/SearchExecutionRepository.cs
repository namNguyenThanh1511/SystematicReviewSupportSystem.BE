using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.SearchExecutionRepo
{
    public class SearchExecutionRepository : GenericRepository<SearchExecution, Guid, AppDbContext>, ISearchExecutionRepository
    {
        public SearchExecutionRepository(AppDbContext context) : base(context)
        {
        }
    }
}
