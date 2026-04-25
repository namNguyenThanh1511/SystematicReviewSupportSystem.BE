using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRSS.IAM.Services.CandidatePaperService;
using SRSS.IAM.Services.CandidatePaperService.DTOs;
using SRSS.IAM.Services.DTOs.Common;
using Shared.Models;
using System.Security.Claims;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CandidatePaperController : ControllerBase
    {
        private readonly ICandidatePaperService _candidatePaperService;

        public CandidatePaperController(ICandidatePaperService candidatePaperService)
        {
            _candidatePaperService = candidatePaperService;
        }

        [HttpPost("papers/{paperId}/extract-references")]
        public async Task<ActionResult<ApiResponse>> ExtractReferences(Guid paperId, CancellationToken cancellationToken)
        {
            await _candidatePaperService.ExtractReferencesFromPaperAsync(paperId, cancellationToken);
            return Ok(new ApiResponse { IsSuccess = true, Message = "References extraction started and saved to candidate pool." });
        }

        [HttpGet("papers/{paperId}/candidate-papers")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<CandidatePaperDto>>>> GetCandidates(Guid paperId, [FromQuery] GetCandidatePapersRequest request, CancellationToken cancellationToken)
        {
            var result = await _candidatePaperService.GetCandidatePapersAsync(paperId, request, cancellationToken);
            return Ok(new ApiResponse<PaginatedResponse<CandidatePaperDto>> { IsSuccess = true, Data = result, Message = "Candidate papers retrieved successfully." });
        }

        [HttpGet("projects/{projectId}/papers/papers-with-candidates")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaperWithCandidateDto>>>> GetPapersWithCandidates(Guid projectId, [FromQuery] GetPapersRequest request, CancellationToken cancellationToken)
        {
            var result = await _candidatePaperService.GetPapersWithCandidatesAsync(projectId, request, cancellationToken);
            return Ok(new ApiResponse<PaginatedResponse<PaperWithCandidateDto>> { IsSuccess = true, Data = result, Message = "Papers with candidate counts retrieved successfully." });
        }

        [HttpGet("papers/{paperId}/candidates")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<CandidatePaperDto>>>> GetCandidatesByPaperId(Guid paperId, [FromQuery] GetCandidatePapersRequest request, CancellationToken cancellationToken)
        {
            var result = await _candidatePaperService.GetCandidatesByPaperIdAsync(paperId, request, cancellationToken);
            return Ok(new ApiResponse<PaginatedResponse<CandidatePaperDto>> { IsSuccess = true, Data = result, Message = "Candidates for the specified paper retrieved successfully." });
        }

        [HttpPost("candidate-papers/reject")]
        public async Task<ActionResult<ApiResponse>> RejectCandidates([FromBody] RejectCandidatePaperRequest request, CancellationToken cancellationToken)
        {
            await _candidatePaperService.RejectCandidatePapersAsync(request, cancellationToken);
            return Ok(new ApiResponse { IsSuccess = true, Message = "Candidate papers rejected." });
        }

        [HttpPost("candidate-papers/select")]
        public async Task<ActionResult<ApiResponse>> SelectCandidates([FromBody] SelectCandidatePaperRequest request, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            await _candidatePaperService.SelectCandidatePapersAsync(request, userId, cancellationToken);
            return Ok(new ApiResponse { IsSuccess = true, Message = "Candidate papers selected and added to project dataset (duplicates skipped)." });
        }
    }
}
