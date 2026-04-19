using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public class StuSeFullTextAiEvaluationBackgroundService : BackgroundService
    {
        private readonly IStuSeFullTextAiEvaluationQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StuSeFullTextAiEvaluationBackgroundService> _logger;

        public StuSeFullTextAiEvaluationBackgroundService(
            IStuSeFullTextAiEvaluationQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<StuSeFullTextAiEvaluationBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StuSeFullTextAiEvaluationBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                StuSeFullTextAiEvaluationTask? task = null;
                try
                {
                    task = await _queue.Reader.ReadAsync(stoppingToken);

                    _logger.LogInformation("Processing Full-Text AI evaluation for StudySelection {Id}, Paper {PaperId}", 
                        task.StudySelectionId, task.PaperId);

                    using var scope = _scopeFactory.CreateScope();
                    var evaluationService = scope.ServiceProvider.GetRequiredService<IStuSeFullTextAiEvaluationService>();

                    await evaluationService.EvaluateFullTextAsync(
                        task.StudySelectionId, 
                        task.PaperId, 
                        task.ReviewerId, 
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Full-Text AI evaluation background job.");
                }
                finally
                {
                    if (task != null)
                    {
                        _queue.Dequeue(task);
                    }
                }
            }

            _logger.LogInformation("StuSeFullTextAiEvaluationBackgroundService is stopping.");
        }
    }
}
