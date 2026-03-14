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
        /// Generate a new PRISMA 2020 flow diagram report for a review process
        /// </summary>
        /// <param name="reviewProcessId">Review Process ID</param>
        /// <param name="request">Report generation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated PRISMA report with flow records</returns>
        [HttpPost("review-processes/{reviewProcessId}/prisma-report")]
        public async Task<ActionResult<ApiResponse<PrismaReportResponse>>> GeneratePrismaReport(
            [FromRoute] Guid reviewProcessId,
            [FromBody] GeneratePrismaReportRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _prismaReportService.GenerateReportAsync(reviewProcessId, request, cancellationToken);
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
        /// Get all PRISMA reports for a specific review process
        /// </summary>
        /// <param name="reviewProcessId">Review Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of PRISMA reports</returns>
        [HttpGet("review-processes/{reviewProcessId}/prisma-reports")]
        public async Task<ActionResult<ApiResponse<List<PrismaReportListResponse>>>> GetPrismaReportsByReviewProcess(
            [FromRoute] Guid reviewProcessId,
            CancellationToken cancellationToken)
        {
            var result = await _prismaReportService.GetReportsByReviewProcessAsync(reviewProcessId, cancellationToken);

            var message = result.Count == 0
                ? "No PRISMA reports found for this review process."
                : $"Retrieved {result.Count} PRISMA report(s).";

            return Ok(result, message);
        }

        /// <summary>
        /// Get the latest PRISMA report for a specific review process
        /// </summary>
        /// <param name="reviewProcessId">Review Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Latest PRISMA report</returns>
        [HttpGet("review-processes/{reviewProcessId}/prisma-report/latest")]
        public async Task<ActionResult<ApiResponse<PrismaReportResponse>>> GetLatestPrismaReport(
            [FromRoute] Guid reviewProcessId,
            CancellationToken cancellationToken)
        {
            var result = await _prismaReportService.GetLatestReportByReviewProcessAsync(reviewProcessId, cancellationToken);
            return Ok(result, "Latest PRISMA report retrieved successfully.");
        }
    }
}
