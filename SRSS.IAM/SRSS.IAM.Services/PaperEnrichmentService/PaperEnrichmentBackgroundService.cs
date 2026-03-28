using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;

namespace SRSS.IAM.Services.PaperEnrichmentService
{
    /// <summary>
    /// Background service that reads paper IDs from the enrichment channel
    /// and performs OpenAlex enrichment for each paper asynchronously.
    /// Creates a new DI scope per paper to handle scoped DbContext correctly.
    /// </summary>
    public class PaperEnrichmentBackgroundService : BackgroundService
    {
        private readonly Channel<Guid> _enrichmentChannel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaperEnrichmentBackgroundService> _logger;

        public PaperEnrichmentBackgroundService(
            Channel<Guid> enrichmentChannel,
            IServiceScopeFactory scopeFactory,
            ILogger<PaperEnrichmentBackgroundService> logger)
        {
            _enrichmentChannel = enrichmentChannel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaperEnrichmentBackgroundService started. Waiting for enrichment requests...");

            await foreach (var paperId in _enrichmentChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessPaperEnrichmentAsync(paperId, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Unhandled error processing enrichment for Paper {PaperId}.", paperId);
                }
            }

            _logger.LogInformation("PaperEnrichmentBackgroundService stopping.");
        }

        private async Task ProcessPaperEnrichmentAsync(Guid paperId, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var enrichmentService = scope.ServiceProvider.GetRequiredService<IPaperEnrichmentService>();

            // Load paper with tracking for updates
            var paper = await unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperId, isTracking: true, ct);

            if (paper == null)
            {
                _logger.LogWarning("Paper {PaperId} not found. Skipping enrichment.", paperId);
                return;
            }

            // Idempotency check
            if (paper.ExternalDataFetched || paper.EnrichmentStatus == EnrichmentStatus.Completed)
            {
                _logger.LogDebug("Paper {PaperId} already enriched. Skipping.", paperId);
                return;
            }

            if (paper.EnrichmentStatus == EnrichmentStatus.Processing)
            {
                _logger.LogDebug("Paper {PaperId} is already being processed. Skipping.", paperId);
                return;
            }

            // Mark as processing
            paper.EnrichmentStatus = EnrichmentStatus.Processing;
            paper.ModifiedAt = DateTimeOffset.UtcNow;
            await unitOfWork.SaveChangesAsync(ct);

            try
            {
                // Perform the actual enrichment via OpenAlex
                await enrichmentService.EnrichFromOpenAlexAsync(paper, ct);

                // EnrichFromOpenAlexAsync sets ExternalDataFetched = true on success
                paper.EnrichmentStatus = paper.ExternalDataFetched
                    ? EnrichmentStatus.Completed
                    : EnrichmentStatus.Failed;

                paper.ModifiedAt = DateTimeOffset.UtcNow;
                await unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Enrichment {Status} for Paper {PaperId}.",
                    paper.EnrichmentStatus, paperId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Enrichment failed for Paper {PaperId}. Marking as Failed.", paperId);

                // Mark as failed — safe for retry later
                paper.EnrichmentStatus = EnrichmentStatus.Failed;
                paper.ModifiedAt = DateTimeOffset.UtcNow;

                try
                {
                    await unitOfWork.SaveChangesAsync(ct);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Failed to save Failed status for Paper {PaperId}.", paperId);
                }
            }
        }
    }
}
