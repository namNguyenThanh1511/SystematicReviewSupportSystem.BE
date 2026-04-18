using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.AuditLogService;
using SRSS.IAM.Services.DTOs.AuditLog;
using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require basic authentication at least
    public class AuditLogController : BaseController
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: api/auditlog/admin
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")] // Uncomment if you have an Admin role defined
        public async Task<ActionResult<ApiResponse<PaginatedResponse<AuditLogResponse>>>> GetAdminLogs(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? user = null,
            [FromQuery] string? actionType = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var logs = await _auditLogService.GetAdminLogsAsync(
                searchTerm, user, actionType, status, startDate, endDate, pageNumber, pageSize, cancellationToken);
            return Ok(logs, "Admin audit logs retrieved successfully");
        }

        // GET: api/auditlog/project-leader/{projectId}
        [HttpGet("project-leader/{projectId}")]
        [Authorize] // Or any relevant authorization mechanism
        public async Task<ActionResult<ApiResponse<PaginatedResponse<AuditLogResponse>>>> GetProjectLeaderLogs(
            Guid projectId,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? user = null,
            [FromQuery] string? actionType = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Ideally, here you would check if the currentUser is actually the leader of projectId
            var logs = await _auditLogService.GetProjectLeaderLogsAsync(
                projectId, searchTerm, user, actionType, status, startDate, endDate, pageNumber, pageSize, cancellationToken);
            return Ok(logs, "Project leader audit logs retrieved successfully");
        }
    }
}
