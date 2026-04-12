using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.StudySelectionService;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.GrobidClient
{
    public class GrobidBackgroundService : BackgroundService
    {
        private readonly IGrobidProcessingQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GrobidBackgroundService> _logger;

        public GrobidBackgroundService(
            IGrobidProcessingQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<GrobidBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GrobidBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                GrobidWorkItem? workItem = null;
                try
                {
                    workItem = await _queue.Reader.ReadAsync(stoppingToken);

                    _logger.LogInformation("Processing GROBID extraction for PaperPdf {PaperPdfId} (Paper: {PaperId})", 
                        workItem.PaperPdfId, workItem.PaperId);

                    using var scope = _scopeFactory.CreateScope();
                    var studySelectionService = scope.ServiceProvider.GetRequiredService<IStudySelectionService>();

                    await studySelectionService.ProcessGrobidExtractionAsync(workItem, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background GROBID extraction for PaperPdf {PaperPdfId}.", 
                        workItem?.PaperPdfId);
                }
            }

            _logger.LogInformation("GrobidBackgroundService is stopping.");
        }
    }
}
