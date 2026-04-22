using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextParsedSectionRepo
{
    public interface IPaperFullTextParsedSectionRepository : IGenericRepository<PaperFullTextParsedSection, Guid, AppDbContext>
    {
    }
}
