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
