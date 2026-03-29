using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;

namespace SRSS.IAM.Services.PaperEnrichmentService
{
    /// <summary>
    /// Orchestrates downstream-driven paper enrichment.
    /// Queries final dataset papers eligible for enrichment and enqueues them
    /// to a Channel for background processing by PaperEnrichmentBackgroundService.
    /// </summary>
    public class PaperEnrichmentOrchestrator : IPaperEnrichmentOrchestrator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Channel<Guid> _enrichmentChannel;
        private readonly ILogger<PaperEnrichmentOrchestrator> _logger;

        public PaperEnrichmentOrchestrator(
            IUnitOfWork unitOfWork,
            Channel<Guid> enrichmentChannel,
            ILogger<PaperEnrichmentOrchestrator> logger)
        {
            _unitOfWork = unitOfWork;
            _enrichmentChannel = enrichmentChannel;
            _logger = logger;
        }

        public async Task TriggerForIdentificationProcessAsync(
            Guid identificationProcessId,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "Starting downstream enrichment trigger for IdentificationProcess {ProcessId}.",
                identificationProcessId);

            // Get final dataset papers that are eligible for enrichment
            var eligiblePapers = await _unitOfWork.Papers
                .GetFinalDatasetPapersForEnrichmentAsync(identificationProcessId, ct);

            if (eligiblePapers.Count == 0)
            {
                _logger.LogInformation(
                    "No papers eligible for enrichment in IdentificationProcess {ProcessId}.",
                    identificationProcessId);
                return;
            }

            _logger.LogInformation(
                "Found {Count} papers eligible for enrichment in IdentificationProcess {ProcessId}. Enqueuing...",
                eligiblePapers.Count, identificationProcessId);

            var enqueuedCount = 0;
            foreach (var paper in eligiblePapers)
            {
                if (_enrichmentChannel.Writer.TryWrite(paper.Id))
                {
                    enqueuedCount++;
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to enqueue Paper {PaperId} for enrichment. Channel may be full.",
                        paper.Id);
                }
            }

            _logger.LogInformation(
                "Enqueued {Count}/{Total} papers for background enrichment from IdentificationProcess {ProcessId}.",
                enqueuedCount, eligiblePapers.Count, identificationProcessId);
        }
    }
}
