using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SRSS.IAM.Services.SupabaseService;

namespace SRSS.IAM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupabaseController : ControllerBase
    {
        private readonly ISupabaseStorageService _storageService;
        public SupabaseController(ISupabaseStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPaper(IFormFile file, [FromForm] Guid projectId)
        {
            var fileUrl = await _storageService.UploadArticlePdfAsync(file, projectId);
            return Ok(new { url = fileUrl });
        }
    }
}
