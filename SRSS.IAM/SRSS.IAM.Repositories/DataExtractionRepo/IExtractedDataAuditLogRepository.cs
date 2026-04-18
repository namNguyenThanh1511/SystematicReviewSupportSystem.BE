using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.DataExtractionRepo
{
    public interface IExtractedDataAuditLogRepository : IGenericRepository<ExtractedDataAuditLog, Guid, AppDbContext>
    {
    }
}
