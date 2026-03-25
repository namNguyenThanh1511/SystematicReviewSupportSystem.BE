using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
    public interface IDataExtractionProcessRepository : IGenericRepository<DataExtractionProcess, Guid, AppDbContext>
    {
        Task<List<DataExtractionProcess>> GetByReviewProcessIdAsync(Guid reviewProcessId);
        IQueryable<DataExtractionProcess> GetQueryable();
    }
}