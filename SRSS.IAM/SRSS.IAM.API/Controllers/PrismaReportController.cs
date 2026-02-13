using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.PrismaReport;
using SRSS.IAM.Services.PrismaReportService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for generating and retrieving PRISMA 2020 flow diagram reports
    /// </summary>
    [ApiController]
    [Route("api")]
    public class PrismaReportController : BaseController
    {
        private readonly IPrismaReportService _prismaReportService;

        public PrismaReportController(IPrismaReportService prismaReportService)
        {
            _prismaReportService = prismaReportService;
        }

        /// <summary>
        /// Generate a new PRISMA 2020 flow diagram report for a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="request">Report generation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated PRISMA report with flow records</returns>
        [HttpPost("projects/{projectId}/prisma-report")]
        public async Task<ActionResult<ApiResponse<PrismaReportResponse>>> GeneratePrismaReport(
            [FromRoute] Guid projectId,
            [FromBody] GeneratePrismaReportRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _prismaReportService.GenerateReportAsync(projectId, request, cancellationToken);
            return Created(result, "PRISMA report generated successfully.");
        }

        /// <summary>
        /// Get a specific PRISMA report by ID
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PRISMA report details</returns>
        [HttpGet("prisma-reports/{id}")]
        public async Task<ActionResult<ApiResponse<PrismaReportResponse>>> GetPrismaReportById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _prismaReportService.GetReportByIdAsync(id, cancellationToken);

            return Ok(result, "PRISMA report retrieved successfully.");
        }

        /// <summary>
        /// Get all PRISMA reports for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of PRISMA reports</returns>
        [HttpGet("projects/{projectId}/prisma-reports")]
        public async Task<ActionResult<ApiResponse<List<PrismaReportListResponse>>>> GetPrismaReportsByProject(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken)
        {
            var result = await _prismaReportService.GetReportsByProjectAsync(projectId, cancellationToken);

            var message = result.Count == 0
                ? "No PRISMA reports found for this project."
                : $"Retrieved {result.Count} PRISMA report(s).";

            return Ok(result, message);
        }

        /// <summary>
        /// Get the latest PRISMA report for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Latest PRISMA report</returns>
        [HttpGet("projects/{projectId}/prisma-report/latest")]
        public async Task<ActionResult<ApiResponse<PrismaReportResponse>>> GetLatestPrismaReport(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken)
        {
            var result = await _prismaReportService.GetLatestReportByProjectAsync(projectId, cancellationToken);

            if (result == null)
            {
                return StatusCode(404, ResponseBuilder.NotFound<PrismaReportResponse>($"No PRISMA reports found for project {projectId}."));
            }

            return Ok(result, "Latest PRISMA report retrieved successfully.");
        }
    }
}
