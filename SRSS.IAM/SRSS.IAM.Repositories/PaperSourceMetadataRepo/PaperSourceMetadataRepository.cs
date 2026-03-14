using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperSourceMetadataRepo
{
    public class PaperSourceMetadataRepository : GenericRepository<PaperSourceMetadata, Guid, AppDbContext>, IPaperSourceMetadataRepository
    {
        public PaperSourceMetadataRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}
