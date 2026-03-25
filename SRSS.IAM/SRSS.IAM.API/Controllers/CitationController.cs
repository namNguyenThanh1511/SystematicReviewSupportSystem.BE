using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.CitationService;
using SRSS.IAM.Services.DTOs.Citation;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/papers")]
    public class CitationController : BaseController
    {
        private readonly ICitationService _citationService;

        public CitationController(ICitationService citationService)
        {
            _citationService = citationService;
        }

        [HttpGet("{id}/references")]
        public async Task<ActionResult<ApiResponse<List<PaperNodeDto>>>> GetReferences(Guid id, CancellationToken cancellationToken)
        {
            var result = await _citationService.GetReferencesAsync(id, cancellationToken);
            return Ok(result, "References retrieved successfully.");
        }

        [HttpGet("{id}/citations")]
        public async Task<ActionResult<ApiResponse<List<PaperNodeDto>>>> GetCitations(Guid id, CancellationToken cancellationToken)
        {
            var result = await _citationService.GetCitationsAsync(id, cancellationToken);
            return Ok(result, "Citations retrieved successfully.");
        }

        [HttpGet("{id}/citation-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetCitationCount(Guid id, CancellationToken cancellationToken)
        {
            var result = await _citationService.GetCitationCountAsync(id, cancellationToken);
            return Ok(result, "Citation count retrieved successfully.");
        }

        [HttpGet("{id}/reference-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetReferenceCount(Guid id, CancellationToken cancellationToken)
        {
            var result = await _citationService.GetReferenceCountAsync(id, cancellationToken);
            return Ok(result, "Reference count retrieved successfully.");
        }

        [HttpGet("{id}/graph")]
        public async Task<ActionResult<ApiResponse<CitationGraphDto>>> GetCitationGraph(
            Guid id, 
            [FromQuery] int depth = 2, 
            [FromQuery] decimal minConfidence = 0.7m, 
            CancellationToken cancellationToken = default)
        {
            var result = await _citationService.GetCitationGraphAsync(id, depth, minConfidence, cancellationToken);
            return Ok(result, "Citation graph retrieved successfully.");
        }

        [HttpGet("top-cited")]
        public async Task<ActionResult<ApiResponse<List<PaperNodeDto>>>> GetTopCitedPapers([FromQuery] int topN = 10, CancellationToken cancellationToken = default)
        {
            var result = await _citationService.GetTopCitedPapersAsync(topN, cancellationToken);
            return Ok(result, "Top cited papers retrieved successfully.");
        }

        [HttpGet("{id}/suggestions")]
        public async Task<ActionResult<ApiResponse<List<PaperNodeDto>>>> GetSuggestedPapers(Guid id, [FromQuery] int limit = 5, CancellationToken cancellationToken = default)
        {
            var result = await _citationService.GetSuggestedPapersAsync(id, limit, cancellationToken);
            return Ok(result, "Suggested papers retrieved successfully.");
        }
    }
}