using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.IdentificationService;
using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/papers")]
    public class PaperController : BaseController
    {
        private readonly IIdentificationService _identificationService;

        public PaperController(IIdentificationService identificationService)
        {
            _identificationService = identificationService;
        }

        /// <summary>
        /// Import bibliographic records from a RIS file
        /// </summary>
        /// <param name="file">RIS file (.ris extension)</param>
        /// <param name="source">Source database (e.g., Scopus, IEEE, PubMed)</param>
        /// <param name="importedBy">User who performed the import</param>
        /// <param name="searchExecutionId">Optional SearchExecution ID to associate papers with</param>
        /// <param name="identificationProcessId">IdentificationProcess ID to associate the import with</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Import summary with counts and any errors</returns>
        [HttpPost("import/ris")]
        public async Task<ActionResult<ApiResponse<RisImportResultDto>>> ImportRisFile(
            IFormFile file,
            [FromForm] string? source,
            [FromForm] string? importedBy,
            [FromForm] Guid? searchExecutionId,
            [FromForm] Guid identificationProcessId,
            CancellationToken cancellationToken)
        {
            // Validate file presence
            if (file == null || file.Length == 0)
            {
                return BadRequest<RisImportResultDto>("No file uploaded.");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".ris")
            {
                return BadRequest<RisImportResultDto>("Invalid file format. Only .ris files are accepted.");
            }

            // Validate file size (e.g., max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                return BadRequest<RisImportResultDto>("File size exceeds the maximum allowed size of 10MB.");
            }

            using var stream = file.OpenReadStream();
            var result = await _identificationService.ImportRisFileAsync(
                stream,
                file.FileName,
                source,
                importedBy,
                searchExecutionId,
                identificationProcessId,
                cancellationToken);

            // Check if import was successful
            if (result.TotalRecords == 0 && result.Errors.Any())
            {
                return BadRequest<RisImportResultDto>("Failed to import RIS file.");
            }

            if (result.ImportedRecords == 0 && result.UpdatedRecords == 0)
            {
                return Ok(result, "No new records imported. All records were duplicates or skipped.");
            }

            return Ok(result, $"Successfully imported {result.ImportedRecords} records.");
        }

        /// <summary>
        /// Import papers manually from JSON payload
        /// </summary>
        [HttpPost("import/manual")]
        public async Task<ActionResult<ApiResponse<ImportPaperResponse>>> ImportManual(
            [FromBody] ImportPaperRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _identificationService.ImportPaperAsync(request, cancellationToken);
            return Ok(result, $"Successfully imported {result.TotalImported} papers.");
        }
    }
}
