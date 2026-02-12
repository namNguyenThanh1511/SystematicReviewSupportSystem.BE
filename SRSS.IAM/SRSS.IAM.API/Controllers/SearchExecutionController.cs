using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.IdentificationService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Search Executions within Identification Processes
    /// </summary>
    [ApiController]
    [Route("api")]
    public class SearchExecutionController : BaseController
    {
        private readonly IIdentificationService _identificationService;

        public SearchExecutionController(IIdentificationService identificationService)
        {
            _identificationService = identificationService;
        }

        /// <summary>
        /// Create a new search execution for an identification process
        /// </summary>
        /// <param name="identificationProcessId">Identification Process ID</param>
        /// <param name="request">Search execution creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created search execution details</returns>
        [HttpPost("identification-processes/{identificationProcessId}/search-executions")]
        public async Task<ActionResult<ApiResponse<SearchExecutionResponse>>> CreateSearchExecution(
            [FromRoute] Guid identificationProcessId,
            [FromBody] CreateSearchExecutionRequest request,
            CancellationToken cancellationToken)
        {
            if (identificationProcessId != request.IdentificationProcessId)
            {
                return BadRequest<SearchExecutionResponse>("IdentificationProcess ID in route does not match ID in request body.");
            }

            try
            {
                var result = await _identificationService.CreateSearchExecutionAsync(request, cancellationToken);
                return Created(result, "Search execution created successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<SearchExecutionResponse>(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SearchExecutionResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Get search execution by ID
        /// </summary>
        /// <param name="id">Search Execution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Search execution details</returns>
        [HttpGet("search-executions/{id}")]
        public async Task<ActionResult<ApiResponse<SearchExecutionResponse>>> GetSearchExecutionById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _identificationService.GetSearchExecutionByIdAsync(id, cancellationToken);

                if (result == null)
                {
                    return NotFound(ResponseBuilder.NotFound<SearchExecutionResponse>($"Search execution with ID {id} not found."));
                }

                return Ok(result, "Search execution retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SearchExecutionResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Get all search executions for a specific identification process
        /// </summary>
        /// <param name="identificationProcessId">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of search executions</returns>
        [HttpGet("identification-processes/{identificationProcessId}/search-executions")]
        public async Task<ActionResult<ApiResponse<List<SearchExecutionResponse>>>> GetSearchExecutionsByIdentificationProcess(
            [FromRoute] Guid identificationProcessId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _identificationService.GetSearchExecutionsByIdentificationProcessIdAsync(identificationProcessId, cancellationToken);
                return Ok(result, "Search executions retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<List<SearchExecutionResponse>>(ex.Message));
            }
        }

        /// <summary>
        /// Update search execution details
        /// </summary>
        /// <param name="id">Search Execution ID</param>
        /// <param name="request">Update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated search execution details</returns>
        [HttpPut("search-executions/{id}")]
        public async Task<ActionResult<ApiResponse<SearchExecutionResponse>>> UpdateSearchExecution(
            [FromRoute] Guid id,
            [FromBody] UpdateSearchExecutionRequest request,
            CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                return BadRequest<SearchExecutionResponse>("ID in route does not match ID in request body.");
            }

            try
            {
                var result = await _identificationService.UpdateSearchExecutionAsync(request, cancellationToken);
                return Ok(result, "Search execution updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ResponseBuilder.NotFound<SearchExecutionResponse>(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError<SearchExecutionResponse>(ex.Message));
            }
        }

        /// <summary>
        /// Delete a search execution
        /// </summary>
        /// <param name="id">Search Execution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpDelete("search-executions/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteSearchExecution(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _identificationService.DeleteSearchExecutionAsync(id, cancellationToken);

                if (!result)
                {
                    return NotFound(ResponseBuilder.NotFound($"Search execution with ID {id} not found."));
                }

                return Ok("Search execution deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseBuilder.InternalServerError(ex.Message));
            }
        }
    }
}
