using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
    public class ExtractionCommentRepository : GenericRepository<ExtractionComment, Guid, AppDbContext>, IExtractionCommentRepository
    {
        public ExtractionCommentRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
