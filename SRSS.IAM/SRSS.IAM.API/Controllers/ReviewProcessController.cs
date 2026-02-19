using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.ReviewProcess;
using SRSS.IAM.Services.ReviewProcessService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Review Processes within Systematic Review Projects
    /// </summary>
    [ApiController]
    [Route("api")]
    public class ReviewProcessController : BaseController
    {
        private readonly IReviewProcessService _reviewProcessService;

        public ReviewProcessController(IReviewProcessService reviewProcessService)
        {
            _reviewProcessService = reviewProcessService;
        }

        /// <summary>
        /// Create a new review process for a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="request">Process creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created process details</returns>
        [HttpPost("projects/{projectId}/review-processes")]
        public async Task<ActionResult<ApiResponse<ReviewProcessResponse>>> CreateReviewProcess(
            [FromRoute] Guid projectId,
            [FromBody] CreateReviewProcessRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _reviewProcessService.CreateReviewProcessAsync(projectId, request, cancellationToken);
            return Created(result, "Review process created successfully.");
        }

        /// <summary>
        /// Get review process by ID
        /// </summary>
        /// <param name="id">Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Process details</returns>
        [HttpGet("review-processes/{id}")]
        public async Task<ActionResult<ApiResponse<ReviewProcessResponse>>> GetReviewProcessById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _reviewProcessService.GetReviewProcessByIdAsync(id, cancellationToken);


            return Ok(result, "Review process retrieved successfully.");
        }

        /// <summary>
        /// Get all review processes for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of processes</returns>
        [HttpGet("projects/{projectId}/review-processes")]
        public async Task<ActionResult<ApiResponse<List<ReviewProcessResponse>>>> GetReviewProcessesByProject(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken)
        {
            var result = await _reviewProcessService.GetReviewProcessesByProjectIdAsync(projectId, cancellationToken);
            return Ok(result, "Review processes retrieved successfully.");
        }

        /// <summary>
        /// Update review process notes
        /// </summary>
        /// <param name="id">Process ID</param>
        /// <param name="request">Update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated process details</returns>
        [HttpPut("review-processes/{id}")]
        public async Task<ActionResult<ApiResponse<ReviewProcessResponse>>> UpdateReviewProcess(
            [FromRoute] Guid id,
            [FromBody] UpdateReviewProcessRequest request,
            CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest<ReviewProcessResponse>("ID in route does not match ID in request body.");
            }

            var result = await _reviewProcessService.UpdateReviewProcessAsync(request, cancellationToken);
            return Ok(result, "Review process updated successfully.");
        }

        /// <summary>
        /// Start a review process (Pending → InProgress). Only one process can be in progress at a time.
        /// </summary>
        /// <param name="id">Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated process details</returns>
        [HttpPost("review-processes/{id}/start")]
        public async Task<ActionResult<ApiResponse<ReviewProcessResponse>>> StartReviewProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _reviewProcessService.StartReviewProcessAsync(id, cancellationToken);
            return Ok(result, "Review process started successfully.");
        }

        /// <summary>
        /// Complete a review process (InProgress → Completed)
        /// </summary>
        /// <param name="id">Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated process details</returns>
        [HttpPost("review-processes/{id}/complete")]
        public async Task<ActionResult<ApiResponse<ReviewProcessResponse>>> CompleteReviewProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _reviewProcessService.CompleteReviewProcessAsync(id, cancellationToken);
            return Ok(result, "Review process completed successfully.");
        }

        /// <summary>
        /// Cancel a review process (Pending/InProgress → Cancelled)
        /// </summary>
        /// <param name="id">Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated process details</returns>
        [HttpPost("review-processes/{id}/cancel")]
        public async Task<ActionResult<ApiResponse<ReviewProcessResponse>>> CancelReviewProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _reviewProcessService.CancelReviewProcessAsync(id, cancellationToken);
            return Ok(result, "Review process cancelled successfully.");
        }

        /// <summary>
        /// Delete a review process
        /// </summary>
        /// <param name="id">Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpDelete("review-processes/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteReviewProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _reviewProcessService.DeleteReviewProcessAsync(id, cancellationToken);

            return Ok("Review process deleted successfully.");
        }
    }
}
