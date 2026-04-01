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
        private readonly IPaperEnrichmentOrchestrator _enrichmentOrchestrator;

        public PaperEnrichmentController(
            IPaperEnrichmentService paperEnrichmentService,
            IPaperEnrichmentOrchestrator enrichmentOrchestrator)
        {
            _paperEnrichmentService = paperEnrichmentService;
            _enrichmentOrchestrator = enrichmentOrchestrator;
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

        /// <summary>
        /// Trigger downstream enrichment for all eligible papers in an identification process.
        /// Papers must have a DOI, not already enriched, and have survived deduplication.
        /// </summary>
        /// <param name="identificationProcessId">The identification process ID.</param>
        /// <param name="ct">Cancellation token.</param>
        [HttpPost("~/api/identification-processes/{identificationProcessId}/enrich")]
        public async Task<ActionResult<ApiResponse<string>>> TriggerEnrichmentForProcess(
            [FromRoute] Guid identificationProcessId,
            CancellationToken ct)
        {
            await _enrichmentOrchestrator.TriggerForIdentificationProcessAsync(identificationProcessId, ct);
            return Ok("Enrichment triggered successfully. Papers are being processed in the background.",
                "Enrichment jobs enqueued for eligible papers.");
        }
    }
}
