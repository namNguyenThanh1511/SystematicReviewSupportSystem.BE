using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
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
            _logger.LogInformation("Starting full-text extraction for PaperPdf {PaperPdfId}", paperPdfId);

            var paperPdf = await _unitOfWork.PaperPdfs.FindSingleAsync(
                p => p.Id == paperPdfId,
                isTracking: true,
                cancellationToken: cancellationToken);

            if (paperPdf == null)
            {
                _logger.LogWarning("PaperPdf {PaperPdfId} not found. Skipping extraction.", paperPdfId);
                return;
            }

            if (paperPdf.FullTextProcessed)
            {
                _logger.LogInformation("Full-text already processed for PaperPdf {PaperPdfId}. Skipping.", paperPdfId);
                return;
            }

            if (string.IsNullOrWhiteSpace(paperPdf.FilePath))
            {
                _logger.LogWarning("PaperPdf {PaperPdfId} has no file path. Skipping extraction.", paperPdfId);
                return;
            }

            try
            {
                // 1. Download PDF from Supabase
                byte[] pdfBytes = await _storageService.DownloadFileAsync(paperPdf.FilePath);
                using var pdfStream = new MemoryStream(pdfBytes);

                // 2. Call GROBID for full-text
                string teiXml = await _grobidService.ProcessFulltextDocumentAsync(pdfStream, cancellationToken);

                if (string.IsNullOrWhiteSpace(teiXml))
                {
                    _logger.LogWarning("GROBID returned empty full-text for PaperPdf {PaperPdfId}.", paperPdfId);
                    return;
                }

                // 3. Save to PaperFullText
                var paperFullText = new PaperFullText
                {
                    Id = Guid.NewGuid(),
                    PaperPdfId = paperPdfId,
                    RawXml = teiXml,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.PaperFullTexts.AddAsync(paperFullText, cancellationToken);

                // 4. Update status
                paperPdf.FullTextProcessed = true;
                paperPdf.ModifiedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully extracted and stored full-text for PaperPdf {PaperPdfId}.", paperPdfId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error extracting full-text for PaperPdf {PaperPdfId}.", paperPdfId);
                // We don't mark as failed here to allow retry later if needed
                throw;
            }
        }
    }
}
