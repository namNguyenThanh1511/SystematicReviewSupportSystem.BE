using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Paper;
using SRSS.IAM.Services.PaperEnrichmentService;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/papers")]
    public class PaperEnrichmentController : BaseController
    {
        private readonly IPaperEnrichmentService _paperEnrichmentService;

        public PaperEnrichmentController(IPaperEnrichmentService paperEnrichmentService)
        {
            _paperEnrichmentService = paperEnrichmentService;
        }

        /// <summary>
        /// Enrich a single paper with external data from OpenAlex.
        /// </summary>
        /// <param name="paperId">The ID of the paper to enrich.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Enrichment result.</returns>
        [HttpPost("{paperId}/enrich-openalex")]
        public async Task<ActionResult<ApiResponse<EnrichPaperResponseDto>>> EnrichSingle(
            [FromRoute] Guid paperId,
            CancellationToken ct)
        {
            var result = await _paperEnrichmentService.EnrichSingleAsync(paperId, ct);
            return Ok(result, "Paper enrichment processed.");
        }

        /// <summary>
        /// Enrich multiple papers in batch with external data from OpenAlex.
        /// </summary>
        /// <param name="paperIds">List of paper IDs to enrich.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Batch enrichment summary.</returns>
        [HttpPost("enrich-openalex/batch")]
        public async Task<ActionResult<ApiResponse<BatchEnrichResponseDto>>> EnrichBatch(
            [FromBody] List<Guid> paperIds,
            CancellationToken ct)
        {
            if (paperIds == null || paperIds.Count == 0)
            {
                throw new ArgumentException("Paper IDs list cannot be empty.");
            }

            var result = await _paperEnrichmentService.EnrichBatchAsync(paperIds, ct);
            return Ok(result, $"Batch enrichment completed: {result.SuccessCount} succeeded, {result.FailedCount} failed.");
        }

        /// <summary>
        /// Enrich all papers missing external data (ExternalDataFetched = false) in small batches.
        /// </summary>
        /// <param name="pageSize">Batch size (default 50).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Batch enrichment summary.</returns>
        [HttpPost("enrich-openalex/missing")]
        public async Task<ActionResult<ApiResponse<BatchEnrichResponseDto>>> EnrichMissing(
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var result = await _paperEnrichmentService.EnrichMissingAsync(pageSize, ct);
            return Ok(result, $"Missing papers enrichment completed for {result.Total} papers.");
        }
    }
}
