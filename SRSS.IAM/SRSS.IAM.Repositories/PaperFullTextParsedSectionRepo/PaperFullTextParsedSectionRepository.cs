using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextParsedSectionRepo
{
    public class PaperFullTextParsedSectionRepository : GenericRepository<PaperFullTextParsedSection, Guid, AppDbContext>, IPaperFullTextParsedSectionRepository
    {
        public PaperFullTextParsedSectionRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
