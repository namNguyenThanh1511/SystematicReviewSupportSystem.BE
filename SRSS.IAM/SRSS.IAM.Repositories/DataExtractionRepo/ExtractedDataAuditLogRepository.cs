using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
    public class ExtractedDataAuditLogRepository : GenericRepository<ExtractedDataAuditLog, Guid, AppDbContext>, IExtractedDataAuditLogRepository
    {
        public ExtractedDataAuditLogRepository(AppDbContext context) : base(context)
        {
        }
    }
}
