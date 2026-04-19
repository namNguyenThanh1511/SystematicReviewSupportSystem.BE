using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;

namespace SRSS.IAM.Services.PaperFullTextService.Chunking
{
    public interface IPaperFullTextChunkingService
    {
        Task ChunkFullTextAsync(Guid paperPdfId, CancellationToken cancellationToken = default);
    }

    public class PaperFullTextChunkingService : IPaperFullTextChunkingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaperFullTextChunkingService> _logger;

        private const int TargetWordCount = 1000; // Default target size

        public PaperFullTextChunkingService(
            IUnitOfWork unitOfWork,
            ILogger<PaperFullTextChunkingService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ChunkFullTextAsync(Guid paperPdfId, CancellationToken cancellationToken = default)
        {
            // --- PHASE 1: Loading & Deleting old chunks ---
            // Load target PaperFullText deterministically (Latest by ModifiedAt)
            var fullText = await _unitOfWork.PaperFullTexts.GetQueryable(ft => ft.PaperPdfId == paperPdfId, isTracking: true)
                .Include(ft => ft.Chunks)
                .OrderByDescending(ft => ft.ModifiedAt)
                .ThenByDescending(ft => ft.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (fullText == null)
            {
                throw new InvalidOperationException($"No PaperFullText found for PaperPdf {paperPdfId}. Extraction must be completed first.");
            }

            Guid fullTextId = fullText.Id;
            _logger.LogInformation("Selected PaperFullText {Id} for chunking PaperPdf {PdfId}.", fullTextId, paperPdfId);

            // Delete existing chunks if any
            if (fullText.Chunks.Any())
            {
                int oldChunkCount = fullText.Chunks.Count;
                _logger.LogInformation("Deleting {Count} old chunks for PaperFullText {Id}.", oldChunkCount, fullTextId);
                
                await _unitOfWork.PaperFullTextChunks.RemoveMultipleAsync(fullText.Chunks.ToList());
                
                // Save changes to commit deletion before inserting new chunks
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully deleted old chunks for PaperFullText {Id}.", fullTextId);
            }

            // Clear EF tracking to ensure phase 2 starts with a truly clean state
            _unitOfWork.ClearTracker();
            _logger.LogDebug("EF Change Tracker cleared after deletion phase.");


            // --- PHASE 2: Generate and Insert new chunks ---
            // Reload parent with parsed sections/paragraphs in a clean state (No tracking needed for read-only generation)
            var refreshedFullText = await _unitOfWork.PaperFullTexts.GetQueryable(ft => ft.Id == fullTextId, isTracking: false)
                .Include(ft => ft.ParsedSections)
                     .ThenInclude(s => s.Paragraphs)
                .FirstOrDefaultAsync(cancellationToken);

            if (refreshedFullText == null)
            {
                throw new InvalidOperationException($"PaperFullText {fullTextId} disappeared after clearing tracker.");
            }

            if (!refreshedFullText.ParsedSections.Any())
            {
                throw new InvalidOperationException($"No parsed sections found for PaperFullText {refreshedFullText.Id}. Parsing must be completed first.");
            }

            _logger.LogInformation("Generating chunks for PaperFullText {Id} from {SectionCount} parsed sections.", fullTextId, refreshedFullText.ParsedSections.Count);

            // Generate new chunk entities in memory
            int globalChunkOrder = 1;
            var newChunks = new List<PaperFullTextChunk>();

            foreach (var section in refreshedFullText.ParsedSections.OrderBy(s => s.Order))
            {
                var currentChunkText = new System.Text.StringBuilder();
                int currentChunkWordCount = 0;

                foreach (var para in section.Paragraphs.OrderBy(p => p.Order))
                {
                    var paraText = para.Text;
                    var paraWordCount = paraText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

                    // If adding this paragraph exceeds the target size significantly
                    if (currentChunkWordCount > 0 && currentChunkWordCount + paraWordCount > TargetWordCount)
                    {
                        newChunks.Add(CreateChunk(fullTextId, globalChunkOrder++, section, currentChunkText.ToString(), currentChunkWordCount));
                        currentChunkText.Clear();
                        currentChunkWordCount = 0;
                    }

                    if (currentChunkText.Length > 0) currentChunkText.Append("\n\n");
                    currentChunkText.Append(paraText);
                    currentChunkWordCount += paraWordCount;
                }

                // Finalize last chunk in section
                if (currentChunkWordCount > 0)
                {
                    newChunks.Add(CreateChunk(fullTextId, globalChunkOrder++, section, currentChunkText.ToString(), currentChunkWordCount));
                }
            }

            _logger.LogInformation("Inserting {Count} new chunks for PaperFullText {Id} directly via repository.", newChunks.Count, fullTextId);
            
            // Insert chunks directly via repository / DbSet, not via refreshedFullText.Chunks.Add(...)
            foreach (var chunk in newChunks)
            {
                await _unitOfWork.PaperFullTextChunks.AddAsync(chunk);
            }

            // Save chunks only
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully inserted {Count} chunks for PaperFullText {Id}.", newChunks.Count, fullTextId);


            // --- PHASE 3: Update parent status ---
            // Clear tracker again to ensure a clean state for status update
            _unitOfWork.ClearTracker();
            _logger.LogDebug("EF Change Tracker cleared after insertion phase.");

            // Reload PaperFullText by Id cleanly
            var statusFullText = await _unitOfWork.PaperFullTexts.FindFirstOrDefaultAsync(ft => ft.Id == fullTextId, isTracking: true, cancellationToken: cancellationToken);
            if (statusFullText == null)
            {
                throw new InvalidOperationException($"PaperFullText {fullTextId} disappeared during status update phase.");
            }

            // Update only status fields
            _logger.LogInformation("Updating status (ChunkedAt/ModifiedAt) for PaperFullText {Id}.", fullTextId);
            statusFullText.ChunkedAt = DateTimeOffset.UtcNow;
            statusFullText.ModifiedAt = DateTimeOffset.UtcNow;

            // Save parent status separately
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully updated status for PaperFullText {Id}. Chunking process complete.", fullTextId);
        }

        private PaperFullTextChunk CreateChunk(Guid fullTextId, int order, PaperFullTextParsedSection section, string text, int wordCount)
        {
            return new PaperFullTextChunk
            {
                Id = Guid.NewGuid(),
                PaperFullTextId = fullTextId,
                Order = order,
                SectionTitle = section.SectionTitle,
                SectionType = section.SectionType,
                Text = text,
                WordCount = wordCount,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
