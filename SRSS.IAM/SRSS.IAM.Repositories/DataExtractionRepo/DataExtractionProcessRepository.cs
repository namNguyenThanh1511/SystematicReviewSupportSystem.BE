using SRSS.IAM.Repositories.Entities;
using Shared.Repositories;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
    public class DataExtractionProcessRepository : GenericRepository<DataExtractionProcess, Guid, AppDbContext>, IDataExtractionProcessRepository
    {
        private readonly AppDbContext _context;
        public DataExtractionProcessRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<DataExtractionProcess>> GetByReviewProcessIdAsync(Guid reviewProcessId)
        {
            return await _context.DataExtractionProcesses
                .Where(p => p.ReviewProcessId == reviewProcessId)
                .ToListAsync();
        }

        public IQueryable<DataExtractionProcess> GetQueryable()
        {
            return _context.DataExtractionProcesses;
        }
    }
}