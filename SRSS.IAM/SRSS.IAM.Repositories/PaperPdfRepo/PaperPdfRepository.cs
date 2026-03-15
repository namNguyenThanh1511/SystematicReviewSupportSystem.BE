using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperPdfRepo
{
    public class PaperPdfRepository : GenericRepository<PaperPdf, Guid, AppDbContext>, IPaperPdfRepository
    {
        public PaperPdfRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
