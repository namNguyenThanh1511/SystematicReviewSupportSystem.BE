using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.PaperFullTextService
{
    public class PaperFullTextBackgroundService : BackgroundService
    {
        private readonly IPaperFullTextQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaperFullTextBackgroundService> _logger;

        public PaperFullTextBackgroundService(
            IPaperFullTextQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<PaperFullTextBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaperFullTextBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _queue.Reader.ReadAsync(stoppingToken);

                    _logger.LogInformation("Processing full-text extraction for PaperPdf {PaperPdfId}", workItem.PaperPdfId);

                    using var scope = _scopeFactory.CreateScope();
                    var fullTextService = scope.ServiceProvider.GetRequiredService<IPaperFullTextService>();

                    await fullTextService.ExtractAndStoreFullTextAsync(workItem, stoppingToken);

                    // 2. Prepare for AI (Parsing -> Chunking -> Embedding)
                    var preparationService = scope.ServiceProvider.GetRequiredService<IPaperFullTextPreparationService>();
                    await preparationService.PrepareForAiAsync(workItem.PaperPdfId, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background full-text extraction.");
                }
            }

            _logger.LogInformation("PaperFullTextBackgroundService is stopping.");
        }
    }
}
