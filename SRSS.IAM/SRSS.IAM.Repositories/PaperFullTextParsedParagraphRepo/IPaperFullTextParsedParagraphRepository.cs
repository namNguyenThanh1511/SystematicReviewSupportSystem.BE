using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextParsedParagraphRepo
{
    public interface IPaperFullTextParsedParagraphRepository : IGenericRepository<PaperFullTextParsedParagraph, Guid, AppDbContext>
    {
    }
}
