using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.SupabaseService;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.PaperFullTextService
{
    public class PaperFullTextService : IPaperFullTextService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly ISupabaseStorageService _storageService;
        private readonly ILogger<PaperFullTextService> _logger;

        public PaperFullTextService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            ISupabaseStorageService storageService,
            ILogger<PaperFullTextService> logger)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task ExtractAndStoreFullTextAsync(Guid paperPdfId, CancellationToken cancellationToken = default)
        {
            await ExtractAndStoreFullTextAsync(new PaperFullTextWorkItem { PaperPdfId = paperPdfId }, cancellationToken);
        }

        public async Task ExtractAndStoreFullTextAsync(PaperFullTextWorkItem workItem, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting full-text extraction for PaperPdf {PaperPdfId}", workItem.PaperPdfId);

            var paperPdf = await _unitOfWork.PaperPdfs.FindSingleAsync(
                p => p.Id == workItem.PaperPdfId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (paperPdf == null)
            {
                _logger.LogWarning("PaperPdf {PaperPdfId} not found. Skipping extraction.", workItem.PaperPdfId);
                return;
            }

            var paper = await _unitOfWork.Papers.FindSingleAsync(
                p => p.Id == paperPdf.PaperId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (paper == null)
            {
                _logger.LogWarning("Paper {PaperId} not found for PaperPdf {PaperPdfId}. Skipping extraction.", paperPdf.PaperId, workItem.PaperPdfId);
                return;
            }

            // Reject stale jobs early so an older upload cannot write against a newer PDF version.
            if (!string.Equals(paperPdf.FileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(paper.CurrentFileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "Skipping full-text extraction for PaperPdf {PaperPdfId} because the queued hash is stale.",
                    workItem.PaperPdfId);
                return;
            }

            if (paperPdf.ValidationStatus != PdfValidationStatus.Valid || paperPdf.ProcessingStatus != PdfProcessingStatus.MetadataValidated)
            {
                _logger.LogInformation(
                    "Skipping full-text extraction for PaperPdf {PaperPdfId} because metadata validation is not complete.",
                    workItem.PaperPdfId);
                return;
            }

            if (paperPdf.FullTextProcessed || paperPdf.ProcessingStatus == PdfProcessingStatus.Completed)
            {
                _logger.LogInformation("Full-text already processed for PaperPdf {PaperPdfId}. Skipping.", workItem.PaperPdfId);
                return;
            }

            if (string.IsNullOrWhiteSpace(paperPdf.FilePath))
            {
                _logger.LogWarning("PaperPdf {PaperPdfId} has no file path. Skipping extraction.", workItem.PaperPdfId);
                return;
            }

            try
            {
                // Mark the job as actively processing before the long-running download/extraction step.
                paperPdf.ProcessingStatus = PdfProcessingStatus.FullTextProcessing;
                paperPdf.ModifiedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 1. Download PDF from Supabase
                byte[] pdfBytes = await _storageService.DownloadFileAsync(paperPdf.FilePath);
                using var pdfStream = new MemoryStream(pdfBytes);

                // 2. Call GROBID for full-text
                string teiXml = await _grobidService.ProcessFulltextDocumentAsync(pdfStream, cancellationToken);

                if (string.IsNullOrWhiteSpace(teiXml))
                {
                    _logger.LogWarning("GROBID returned empty full-text for PaperPdf {PaperPdfId}.", workItem.PaperPdfId);
                    return;
                }

                // Re-check the latest state before persisting so a newer upload cannot be overwritten by this late job.
                var latestPaperPdf = await _unitOfWork.PaperPdfs.FindSingleAsync(
                    p => p.Id == workItem.PaperPdfId,
                    isTracking: false,
                    cancellationToken: cancellationToken);

                var latestPaper = await _unitOfWork.Papers.FindSingleAsync(
                    p => p.Id == paperPdf.PaperId,
                    isTracking: false,
                    cancellationToken: cancellationToken);

                if (latestPaperPdf == null || latestPaper == null ||
                    !string.Equals(latestPaperPdf.FileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(latestPaper.CurrentFileHash, workItem.FileHash, StringComparison.OrdinalIgnoreCase) ||
                    latestPaperPdf.ValidationStatus != PdfValidationStatus.Valid)
                {
                    _logger.LogInformation(
                        "Skipping full-text persistence for PaperPdf {PaperPdfId} because the file version changed during processing.",
                        workItem.PaperPdfId);
                    return;
                }

                // 3. Save to PaperFullText
                var paperFullText = new PaperFullText
                {
                    Id = Guid.NewGuid(),
                    PaperPdfId = workItem.PaperPdfId,
                    RawXml = teiXml,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.PaperFullTexts.AddAsync(paperFullText, cancellationToken);

                // 4. Update status
                paperPdf.FullTextProcessed = true;
                paperPdf.ProcessingStatus = PdfProcessingStatus.Completed;
                paperPdf.FullTextProcessedAt = DateTimeOffset.UtcNow;
                paperPdf.ModifiedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully extracted and stored full-text for PaperPdf {PaperPdfId}.", workItem.PaperPdfId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                paperPdf.ProcessingStatus = PdfProcessingStatus.Failed;
                paperPdf.ModifiedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogError(ex, "Error extracting full-text for PaperPdf {PaperPdfId}.", workItem.PaperPdfId);
                throw;
            }
        }
    }
}
