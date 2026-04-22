using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextParsedParagraphRepo
{
    public class PaperFullTextParsedParagraphRepository : GenericRepository<PaperFullTextParsedParagraph, Guid, AppDbContext>, IPaperFullTextParsedParagraphRepository
    {
        public PaperFullTextParsedParagraphRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
