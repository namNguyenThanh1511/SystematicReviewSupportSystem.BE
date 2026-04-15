using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.AuditLog;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.AuditLogService
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        private static readonly string[] AdminLogEntities = new[] 
        { 
            "SystematicReviewProject", 
            "User", 
            "ProjectMember" 
        };

        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponse<AuditLogResponse>> GetAdminLogsAsync(
            string? searchTerm = null,
            string? user = null,
            string? actionType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _unitOfWork.AuditLogs.GetQueryable()
                .Where(log => AdminLogEntities.Contains(log.ResourceType));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(log => 
                    (log.Action != null && log.Action.ToLower().Contains(lowerSearch)) ||
                    (log.ActionType != null && log.ActionType.ToLower().Contains(lowerSearch)) ||
                    (log.ResourceType != null && log.ResourceType.ToLower().Contains(lowerSearch))
                );
            }

            if (!string.IsNullOrEmpty(user))
            {
                var lowerUser = user.ToLower();
                query = query.Where(log => 
                    (log.UserId != null && log.UserId.ToLower().Contains(lowerUser)) || 
                    (log.UserName != null && log.UserName.ToLower().Contains(lowerUser))
                );
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                var lowerAction = actionType.ToLower();
                query = query.Where(log => log.ActionType != null && log.ActionType.ToLower() == lowerAction);
            }

            if (!string.IsNullOrEmpty(status))
            {
                var lowerStatus = status.ToLower();
                query = query.Where(log => log.Status != null && log.Status.ToLower() == lowerStatus);
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<AuditLogResponse>
            {
                Items = logs.Select(log => log.ToResponse()).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponse<AuditLogResponse>> GetProjectLeaderLogsAsync(
            Guid projectId,
            string? searchTerm = null,
            string? user = null,
            string? actionType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _unitOfWork.AuditLogs.GetQueryable()
                .Where(log => !AdminLogEntities.Contains(log.ResourceType) && log.ProjectId == projectId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(log => 
                    (log.Action != null && log.Action.ToLower().Contains(lowerSearch)) ||
                    (log.ActionType != null && log.ActionType.ToLower().Contains(lowerSearch)) ||
                    (log.ResourceType != null && log.ResourceType.ToLower().Contains(lowerSearch))
                );
            }

            if (!string.IsNullOrEmpty(user))
            {
                var lowerUser = user.ToLower();
                query = query.Where(log => 
                    (log.UserId != null && log.UserId.ToLower().Contains(lowerUser)) || 
                    (log.UserName != null && log.UserName.ToLower().Contains(lowerUser))
                );
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                var lowerAction = actionType.ToLower();
                query = query.Where(log => log.ActionType != null && log.ActionType.ToLower() == lowerAction);
            }

            if (!string.IsNullOrEmpty(status))
            {
                var lowerStatus = status.ToLower();
                query = query.Where(log => log.Status != null && log.Status.ToLower() == lowerStatus);
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<AuditLogResponse>
            {
                Items = logs.Select(log => log.ToResponse()).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task CreateAuditLogAsync(AuditLog log)
        {
            await _unitOfWork.AuditLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
