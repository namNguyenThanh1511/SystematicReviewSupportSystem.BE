using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.IdentificationService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// API endpoints for managing Import Batches
    /// </summary>
    [ApiController]
    [Route("api")]
    public class ImportBatchController : BaseController
    {
        private readonly IIdentificationService _identificationService;

        public ImportBatchController(IIdentificationService identificationService)
        {
            _identificationService = identificationService;
        }

        /// <summary>
        /// Create a new import batch for a search execution
        /// </summary>
        /// <param name="searchExecutionId">Search Execution ID</param>
        /// <param name="request">Import batch creation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created import batch details</returns>
        [HttpPost("search-executions/{searchExecutionId}/import-batches")]
        public async Task<ActionResult<ApiResponse<ImportBatchResponse>>> CreateImportBatch(
            [FromRoute] Guid searchExecutionId,
            [FromBody] CreateImportBatchRequest request,
            CancellationToken cancellationToken)
        {
            if (searchExecutionId != request.SearchExecutionId)
            {
                throw new ArgumentException("SearchExecution ID in route does not match ID in request body.");
            }

            var result = await _identificationService.CreateImportBatchAsync(request, cancellationToken);
            return Created(result, "Import batch created successfully.");
        }

        /// <summary>
        /// Get import batch by ID
        /// </summary>
        /// <param name="id">Import Batch ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Import batch details</returns>
        [HttpGet("import-batches/{id}")]
        public async Task<ActionResult<ApiResponse<ImportBatchResponse>>> GetImportBatchById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.GetImportBatchByIdAsync(id, cancellationToken);
            return Ok(result, "Import batch retrieved successfully.");
        }

        /// <summary>
        /// Get all import batches for a specific search execution
        /// </summary>
        /// <param name="searchExecutionId">Search Execution ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of import batches</returns>
        [HttpGet("search-executions/{searchExecutionId}/import-batches")]
        public async Task<ActionResult<ApiResponse<List<ImportBatchResponse>>>> GetImportBatchesBySearchExecution(
            [FromRoute] Guid searchExecutionId,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.GetImportBatchesBySearchExecutionIdAsync(searchExecutionId, cancellationToken);
            return Ok(result, "Import batches retrieved successfully.");
        }

        /// <summary>
        /// Get all import batches for a specific identification process
        /// </summary>
        /// <param name="identificationProcessId">Identification Process ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of import batches</returns>
        [HttpGet("identification-processes/{identificationProcessId}/import-batches")]
        public async Task<ActionResult<ApiResponse<List<ImportBatchResponse>>>> GetImportBatchesByIdentificationProcess(
            [FromRoute] Guid identificationProcessId,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.GetImportBatchesByIdentificationProcessIdAsync(identificationProcessId, cancellationToken);
            return Ok(result, "Import batches retrieved successfully.");
        }

        /// <summary>
        /// Update import batch details
        /// </summary>
        /// <param name="id">Import Batch ID</param>
        /// <param name="request">Update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated import batch details</returns>
        [HttpPut("import-batches/{id}")]
        public async Task<ActionResult<ApiResponse<ImportBatchResponse>>> UpdateImportBatch(
            [FromRoute] Guid id,
            [FromBody] UpdateImportBatchRequest request,
            CancellationToken cancellationToken)
        {
            if (id != request.Id)
            {
                throw new ArgumentException("ID in route does not match ID in request body.");
            }

            var result = await _identificationService.UpdateImportBatchAsync(request, cancellationToken);
            return Ok(result, "Import batch updated successfully.");
        }

        /// <summary>
        /// Delete an import batch
        /// </summary>
        /// <param name="id">Import Batch ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success status</returns>
        [HttpDelete("import-batches/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteImportBatch(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            await _identificationService.DeleteImportBatchAsync(id, cancellationToken);
            return Ok("Import batch deleted successfully.");
        }

        /// <summary>
        /// Get all papers for a specific import batch
        /// </summary>
        /// <param name="id">Import Batch ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of papers in the import batch</returns>
        [HttpGet("import-batches/{id}/papers")]
        public async Task<ActionResult<ApiResponse<List<PaperResponse>>>> GetPapersByImportBatch(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.GetPapersByImportBatchIdAsync(id, cancellationToken);
            return Ok(result, $"Retrieved {result.Count} papers from import batch.");
        }
    }
}
