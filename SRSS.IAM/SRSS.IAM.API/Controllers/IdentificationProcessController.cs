using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.IdentificationService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Identification Process lifecycle
    /// </summary>
    [ApiController]
    [Route("api")]
    public class IdentificationProcessController : BaseController
    {
        private readonly IIdentificationService _identificationService;

        public IdentificationProcessController(IIdentificationService identificationService)
        {
            _identificationService = identificationService;
        }

        /// <summary>
        /// Create a new Identification Process for a Review Process
        /// </summary>
        /// <param name="reviewProcessId">Review Process ID</param>
        /// <param name="request">Creation request with notes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created Identification Process details</returns>
        [HttpPost("review-processes/{reviewProcessId}/identification")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> CreateIdentificationProcess(
            [FromRoute] Guid reviewProcessId,
            [FromBody] CreateIdentificationProcessRequest request,
            CancellationToken cancellationToken)
        {
            request.ReviewProcessId = reviewProcessId;
            var result = await _identificationService.CreateIdentificationProcessAsync(request, cancellationToken);
            return Created(result, "Identification Process created successfully.");
        }

        /// <summary>
        /// Get Identification Process by ID
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Identification Process details</returns>
        [HttpGet("identification-processes/{id}")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> GetIdentificationProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.GetIdentificationProcessByIdAsync(id, cancellationToken);
            return Ok(result, "Identification Process retrieved successfully.");
        }

        /// <summary>
        /// Start Identification Process (transitions from NotStarted to InProgress)
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated Identification Process details</returns>
        [HttpPost("identification-processes/{id}/start")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> StartIdentificationProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.StartIdentificationProcessAsync(id, cancellationToken);
            return Ok(result, "Identification Process started successfully.");
        }

        /// <summary>
        /// Complete Identification Process (transitions from InProgress to Completed)
        /// </summary>
        /// <param name="id">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated Identification Process details</returns>
        [HttpPost("identification-processes/{id}/complete")]
        public async Task<ActionResult<ApiResponse<IdentificationProcessResponse>>> CompleteIdentificationProcess(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.CompleteIdentificationProcessAsync(id, cancellationToken);
            return Ok(result, "Identification Process completed successfully.");
        }
    }
}
