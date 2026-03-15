using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.StudySelectionService;
using SRSS.IAM.Services.SupabaseService;

namespace SRSS.IAM.API.Controllers
{
    /// <summary>
    /// Combined endpoint for uploading a PDF to Supabase Storage
    /// and updating the Paper full-text link in a single request.
    /// </summary>
    [ApiController]
    [Route("api/paper-fulltext")]
    public class PaperFullTextUploadController : BaseController
    {
        private readonly ISupabaseStorageService _storageService;
        private readonly IStudySelectionService _studySelectionService;

        public PaperFullTextUploadController(
            ISupabaseStorageService storageService,
            IStudySelectionService studySelectionService)
        {
            _storageService = storageService;
            _studySelectionService = studySelectionService;
        }

        /// <summary>
        /// Upload a PDF file to Supabase Storage and update the Paper's PdfUrl in one step.
        /// </summary>
        /// <param name="file">The PDF file to upload (max 20 MB)</param>
        /// <param name="projectId">The project ID for storage path organization</param>
        /// <param name="processId">The Study Selection Process ID</param>
        /// <param name="paperId">The Paper ID to update with the uploaded PDF URL</param>
        /// <param name="extractWithGrobid">Whether to extract GROBID metadata during upload</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated paper with decisions</returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<PaperWithDecisionsResponse>>> UploadPaperFullText(
            [FromForm] UploadPaperFullTextRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validation — exceptions handled by GlobalExceptionMiddleware
            if (request.File == null || request.File.Length == 0)
            {
                throw new ArgumentException("File is required and must not be empty.");
            }

            var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (fileExtension != ".pdf")
            {
                throw new ArgumentException("Only PDF files are accepted.");
            }

            if (request.File.Length > 20 * 1024 * 1024)
            {
                throw new ArgumentException("File size must not exceed 20 MB.");
            }

            // Step 1: Upload PDF to Supabase Storage
            var uploadedUrl = await _storageService.UploadArticlePdfAsync(request.File, request.ProjectId, request.ProcessId);

            // Step 2: Update Paper.PdfUrl in the database and optionally extract metadata
            await using var stream = request.ExtractWithGrobid ? request.File.OpenReadStream() : null;

            var updatePaperFullTextRequest = new UpdatePaperFullTextRequest
            {
                PdfUrl = uploadedUrl,
                PdfFileName = request.File.FileName,
                ExtractWithGrobid = request.ExtractWithGrobid,
                PdfStream = stream
            };

            var result = await _studySelectionService.UpdatePaperFullTextAsync(
                request.ProcessId, request.PaperId, updatePaperFullTextRequest, cancellationToken);

            // Step 3: Return updated paper
            return Ok(result, "Paper full-text PDF uploaded and linked successfully.");
        }

        /// <summary>
        /// Retry metadata extraction for a paper that already has a PDF uploaded.
        /// </summary>
        /// <param name="processId">The Study Selection Process ID</param>
        /// <param name="paperId">The Paper ID to retry extraction for</param>
        /// <param name="request">Retry extraction configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated paper with decisions</returns>
        [HttpPost("{paperId}/extract-metadata")]
        public async Task<ActionResult<ApiResponse<PaperWithDecisionsResponse>>> RetryMetadataExtraction(
            [FromRoute] Guid processId,
            [FromRoute] Guid paperId,
            [FromBody] RetryExtractionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _studySelectionService.RetryMetadataExtractionAsync(
                processId, paperId, request, cancellationToken);

            return Ok(result, "Metadata extraction retry completed.");
        }
    }
}
