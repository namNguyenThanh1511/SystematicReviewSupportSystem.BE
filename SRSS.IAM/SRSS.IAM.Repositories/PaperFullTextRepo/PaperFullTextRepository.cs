using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperFullTextRepo
{
    public class PaperFullTextRepository : GenericRepository<PaperFullText, Guid, AppDbContext>, IPaperFullTextRepository
    {
        public PaperFullTextRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
