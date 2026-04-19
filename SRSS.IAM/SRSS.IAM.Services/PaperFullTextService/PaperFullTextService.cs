using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.SupabaseService;
using SRSS.IAM.Services.PaperFullTextService.Parser;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.PaperFullTextService
{
    public class PaperFullTextService : IPaperFullTextService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGrobidService _grobidService;
        private readonly ISupabaseStorageService _storageService;
        private readonly ITeiXmlParser _xmlParser;
        private readonly ILogger<PaperFullTextService> _logger;

        public PaperFullTextService(
            IUnitOfWork unitOfWork,
            IGrobidService grobidService,
            ISupabaseStorageService storageService,
            ITeiXmlParser xmlParser,
            ILogger<PaperFullTextService> logger)
        {
            _unitOfWork = unitOfWork;
            _grobidService = grobidService;
            _storageService = storageService;
            _xmlParser = xmlParser;
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

                _logger.LogInformation("Successfully extracted and stored full-text for PaperPdf {PaperPdfId}. Proceeding to parsing and preparation.", paperPdfId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error extracting full-text for PaperPdf {PaperPdfId}.", paperPdfId);
                // We don't mark as failed here to allow retry later if needed
                throw;
            }
        }

        public async Task ParseFullTextAsync(Guid paperPdfId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting parsing of full-text for PaperPdf {PaperPdfId}", paperPdfId);

            // 1. Load target PaperFullText deterministically (Latest by ModifiedAt)
            var fullText = await _unitOfWork.PaperFullTexts.GetQueryable(ft => ft.PaperPdfId == paperPdfId, isTracking: true)
                .Include(ft => ft.ParsedSections)
                    .ThenInclude(s => s.Paragraphs)
                .OrderByDescending(ft => ft.ModifiedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (fullText == null)
            {
                throw new InvalidOperationException($"No PaperFullText found for PaperPdf {paperPdfId}. Extraction must be completed first.");
            }

            _logger.LogDebug("Target PaperFullText ID: {PaperFullTextId} for PaperPdf {PaperPdfId}", fullText.Id, paperPdfId);

            if (string.IsNullOrWhiteSpace(fullText.RawXml))
            {
                _logger.LogWarning("RawXml is empty for PaperFullText {PaperFullTextId}. Cannot parse.", fullText.Id);
                throw new InvalidOperationException($"RawXml is empty for PaperFullText {fullText.Id}.");
            }

            // 2. Parse XML into DTO
            var parsedDto = _xmlParser.Parse(fullText.RawXml);

            // 3. PHASE 1: Delete existing parsed structure
            if (fullText.ParsedSections.Any())
            {
                var existingParagraphs = fullText.ParsedSections
                    .SelectMany(s => s.Paragraphs)
                    .ToList();

                int oldSectionCount = fullText.ParsedSections.Count;
                int oldParaCount = existingParagraphs.Count;

                _logger.LogInformation("Deleting existing parsed structure for PaperFullText {PaperFullTextId}: {SectionCount} sections, {ParaCount} paragraphs.",
                    fullText.Id, oldSectionCount, oldParaCount);

                // Explicitly remove paragraphs first to avoid FK issues or inconsistent graph states
                if (existingParagraphs.Any())
                {
                    await _unitOfWork.PaperFullTextParsedParagraphs.RemoveMultipleAsync(existingParagraphs);
                }

                // Then remove sections
                await _unitOfWork.PaperFullTextParsedSections.RemoveMultipleAsync(fullText.ParsedSections.ToList());

                // Save changes to commit deletion before inserting new data
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 4. PHASE 2: Rebuild and Insert
            // Reload the PaperFullText to ensure clean tracking and empty collections for new entities
            var refreshedFullText = await _unitOfWork.PaperFullTexts.GetQueryable(ft => ft.Id == fullText.Id, isTracking: true)
                .FirstOrDefaultAsync(cancellationToken);

            if (refreshedFullText == null)
            {
                throw new InvalidOperationException($"PaperFullText {fullText.Id} was lost during reparsing.");
            }

            // Rebuild parsed structure from scratch
            int newSectionsCount = 0;
            int newParaCount = 0;
            foreach (var sectionDto in parsedDto.Sections)
            {
                var sectionEntity = new PaperFullTextParsedSection
                {
                    Id = Guid.NewGuid(),
                    PaperFullTextId = refreshedFullText.Id,
                    Order = sectionDto.Order,
                    SectionTitle = sectionDto.SectionTitle,
                    SectionType = sectionDto.SectionType,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                foreach (var paraDto in sectionDto.Paragraphs)
                {
                    sectionEntity.Paragraphs.Add(new PaperFullTextParsedParagraph
                    {
                        Id = Guid.NewGuid(),
                        SectionId = sectionEntity.Id,
                        Order = paraDto.Order,
                        Text = paraDto.Text,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    });
                    newParaCount++;
                }

                await _unitOfWork.PaperFullTextParsedSections.AddAsync(sectionEntity, cancellationToken);
                newSectionsCount++;
            }

            // 5. Update status and save
            refreshedFullText.ParsedAt = DateTimeOffset.UtcNow;
            refreshedFullText.ModifiedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            bool abstractFound = parsedDto.Sections.Any(s => s.SectionType == "Abstract");

            _logger.LogInformation("Successfully parsed and replaced full-text for PaperPdf {PaperPdfId}. " +
                "Summary: PaperFullTextId={PaperFullTextId}, AbstractFound={AbstractFound}, SectionsAdd={SectionCount}, ParagraphsAdd={ParaCount}",
                paperPdfId, refreshedFullText.Id, abstractFound, newSectionsCount, newParaCount);
        }
    }
}
