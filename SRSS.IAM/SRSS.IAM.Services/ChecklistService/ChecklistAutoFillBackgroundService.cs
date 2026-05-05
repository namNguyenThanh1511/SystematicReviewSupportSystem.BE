using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.DTOs.Checklist;
using SRSS.IAM.Services.NotificationService;

namespace SRSS.IAM.Services.ChecklistService
{
    public class ChecklistAutoFillBackgroundService : BackgroundService
    {
        private readonly IChecklistAutoFillQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChecklistAutoFillBackgroundService> _logger;

        public ChecklistAutoFillBackgroundService(
            IChecklistAutoFillQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<ChecklistAutoFillBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ChecklistAutoFillBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                ChecklistAutoFillWorkItem? workItem = null;
                try
                {
                    workItem = await _queue.Reader.ReadAsync(stoppingToken);

                    _logger.LogInformation(
                        "Processing checklist auto-fill for ReviewChecklist {ReviewChecklistId} (User: {UserId}, File: {FileName})",
                        workItem.ReviewChecklistId, workItem.UserId, workItem.FileName);

                    using var scope = _scopeFactory.CreateScope();
                    var checklistService = scope.ServiceProvider.GetRequiredService<IReviewChecklistService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    await ProcessAutoFillAsync(workItem, checklistService, notificationService, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background checklist auto-fill for ReviewChecklist {ReviewChecklistId}.",
                        workItem?.ReviewChecklistId);

                    // Send failure notification if we have user info
                    if (workItem != null)
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                            await notificationService.SendChecklistAutoFillStatusAsync(workItem.UserId, new ChecklistAutoFillStatusDto
                            {
                                ReviewChecklistId = workItem.ReviewChecklistId,
                                Status = AutoFillStatus.Failed,
                                Message = $"Auto-fill failed: {ex.Message}",
                                Timestamp = DateTimeOffset.UtcNow
                            });
                        }
                        catch (Exception notifyEx)
                        {
                            _logger.LogError(notifyEx, "Failed to send failure notification for checklist auto-fill.");
                        }
                    }
                }
            }

            _logger.LogInformation("ChecklistAutoFillBackgroundService is stopping.");
        }

        private async Task ProcessAutoFillAsync(
            ChecklistAutoFillWorkItem workItem,
            IReviewChecklistService checklistService,
            INotificationService notificationService,
            CancellationToken cancellationToken)
        {
            using var pdfStream = new MemoryStream(workItem.PdfBytes);
            await checklistService.AutoFillChecklistFromPdfAsync(
                workItem.ReviewChecklistId,
                pdfStream,
                workItem.FileName,
                workItem.UserId,
                cancellationToken);
        }
    }
}
