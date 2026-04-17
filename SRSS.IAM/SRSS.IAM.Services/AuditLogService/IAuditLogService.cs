using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.AuditLog;
using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.AuditLogService
{
    public interface IAuditLogService
    {
        Task<PaginatedResponse<AuditLogResponse>> GetAdminLogsAsync(
            string? searchTerm = null,
            string? user = null,
            string? actionType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1, 
            int pageSize = 10, 
            CancellationToken cancellationToken = default);
            
        Task<PaginatedResponse<AuditLogResponse>> GetProjectLeaderLogsAsync(
            Guid projectId,
            string? searchTerm = null,
            string? user = null,
            string? actionType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1, 
            int pageSize = 10, 
            CancellationToken cancellationToken = default);
        Task CreateAuditLogAsync(AuditLog log);

        Task AppendCustomAuditLogAsync(
            Guid projectId, 
            string action, 
            string actionType, 
            string resourceType, 
            string resourceId, 
            object? oldValue = null, 
            object? newValue = null, 
            List<string>? affectedColumns = null);

        void IgnoreTable(string tableName);
    }
}
