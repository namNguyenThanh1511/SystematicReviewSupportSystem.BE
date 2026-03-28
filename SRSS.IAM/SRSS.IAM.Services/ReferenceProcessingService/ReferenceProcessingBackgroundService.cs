using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SRSS.IAM.Services.ReferenceProcessingService
{
    public class ReferenceProcessingBackgroundService : BackgroundService
    {
        private readonly Channel<ReferenceProcessingJob> _jobChannel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReferenceProcessingBackgroundService> _logger;

        public ReferenceProcessingBackgroundService(
            Channel<ReferenceProcessingJob> jobChannel,
            IServiceScopeFactory scopeFactory,
            ILogger<ReferenceProcessingBackgroundService> logger)
        {
            _jobChannel = jobChannel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReferenceProcessingBackgroundService started. Waiting for processing jobs...");

            await foreach (var job in _jobChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessJobAsync(job, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Unhandled error processing candidates for Paper {PaperId} in Process {ProcessId}.", job.PaperId, job.ProcessId);
                }
            }

            _logger.LogInformation("ReferenceProcessingBackgroundService stopping.");
        }

        private async Task ProcessJobAsync(ReferenceProcessingJob job, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var processingService = scope.ServiceProvider.GetRequiredService<IReferenceProcessingService>();

            _logger.LogInformation("Background processing started for candidates of Paper {PaperId} in Process {ProcessId}.", job.PaperId, job.ProcessId);
            
            await processingService.ProcessCandidatesAsync(job.ProcessId, job.PaperId, ct);

            _logger.LogInformation("Background processing completed for candidates of Paper {PaperId} in Process {ProcessId}.", job.PaperId, job.ProcessId);
        }
    }
}
