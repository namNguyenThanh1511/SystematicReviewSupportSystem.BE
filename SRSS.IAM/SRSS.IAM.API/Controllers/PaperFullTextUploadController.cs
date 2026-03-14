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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated paper with decisions</returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<PaperWithDecisionsResponse>>> UploadPaperFullText(
            [FromForm] IFormFile file,
            [FromForm] Guid projectId,
            [FromForm] Guid processId,
            [FromForm] Guid paperId,
            CancellationToken cancellationToken)
        {
            // Validation — exceptions handled by GlobalExceptionMiddleware
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is required and must not be empty.");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileExtension != ".pdf")
            {
                throw new ArgumentException("Only PDF files are accepted.");
            }

            if (file.Length > 20 * 1024 * 1024)
            {
                throw new ArgumentException("File size must not exceed 20 MB.");
            }

            // Step 1: Upload PDF to Supabase Storage
            var uploadedUrl = await _storageService.UploadArticlePdfAsync(file, projectId, processId);

            // Step 2: Update Paper.PdfUrl in the database
            var request = new UpdatePaperFullTextRequest
            {
                PdfUrl = uploadedUrl,
                PdfFileName = file.FileName
            };

            var result = await _studySelectionService.UpdatePaperFullTextAsync(
                processId, paperId, request, cancellationToken);

            // Step 3: Return updated paper
            return Ok(result, "Paper full-text PDF uploaded and linked successfully.");
        }
    }
}
