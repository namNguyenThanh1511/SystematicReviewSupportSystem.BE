using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.EmbeddingService;

namespace SRSS.IAM.Services.PaperFullTextService.Embedding
{
    public interface IPaperFullTextChunkEmbeddingService
    {
        Task EmbedChunksAsync(Guid paperPdfId, CancellationToken cancellationToken = default);
    }

    public class PaperFullTextChunkEmbeddingService : IPaperFullTextChunkEmbeddingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<PaperFullTextChunkEmbeddingService> _logger;

        public PaperFullTextChunkEmbeddingService(
            IUnitOfWork unitOfWork,
            IEmbeddingService embeddingService,
            ILogger<PaperFullTextChunkEmbeddingService> logger)
        {
            _unitOfWork = unitOfWork;
            _embeddingService = embeddingService;
            _logger = logger;
        }

        public async Task EmbedChunksAsync(Guid paperPdfId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting embedding generation for PaperPdf {PaperPdfId}", paperPdfId);

            // 1. Load target PaperFullText deterministically (Latest by ModifiedAt)
            var fullText = await _unitOfWork.PaperFullTexts.GetQueryable(ft => ft.PaperPdfId == paperPdfId, isTracking: true)
                .Include(ft => ft.Chunks)
                    .ThenInclude(c => c.Embedding)
                .OrderByDescending(ft => ft.ModifiedAt)
                .ThenByDescending(ft => ft.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (fullText == null)
            {
                throw new InvalidOperationException($"No PaperFullText found for PaperPdf {paperPdfId}. Extraction must be completed first.");
            }

            _logger.LogInformation("Selected PaperFullText {Id} for embedding PaperPdf {PdfId}.", fullText.Id, paperPdfId);

            // PHASE 1: Remove existing embeddings (Replacement strategy)
            var existingEmbeddings = fullText.Chunks
                .Where(c => c.Embedding != null)
                .Select(c => c.Embedding!)
                .ToList();

            if (existingEmbeddings.Any())
            {
                _logger.LogInformation("Deleting {Count} old embeddings for PaperFullText {Id}.", existingEmbeddings.Count, fullText.Id);
                await _unitOfWork.PaperFullTextChunkEmbeddings.RemoveMultipleAsync(existingEmbeddings);
                
                // Save changes to commit deletion before generating new ones
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully deleted old embeddings for PaperFullText {Id}.", fullText.Id);
            }

            // PHASE 2: Clear EF tracking to ensure clean reload
            _unitOfWork.ClearTracker();
            _logger.LogDebug("EF Change Tracker cleared for clean reload.");

            // PHASE 3: Reload PaperFullText to avoid stale state
            var refreshedFullText = await _unitOfWork.PaperFullTexts.GetQueryable(ft => ft.Id == fullText.Id, isTracking: true)
                .Include(ft => ft.Chunks)
                .FirstOrDefaultAsync(cancellationToken);

            if (refreshedFullText == null)
            {
                throw new InvalidOperationException($"PaperFullText {fullText.Id} disappeared after clearing tracker.");
            }

            if (!refreshedFullText.Chunks.Any())
            {
                throw new InvalidOperationException($"No chunks found for PaperFullText {refreshedFullText.Id}. Chunking must be completed first.");
            }

            // PHASE 4: Generate and insert embeddings directly using repository
            var modelName = _embeddingService.ModelName;
            _logger.LogInformation("Generating embeddings for {Count} chunks using model {Model}.", refreshedFullText.Chunks.Count, modelName);

            foreach (var chunk in refreshedFullText.Chunks.OrderBy(c => c.Order))
            {
                try
                {
                    var vectorArray = await _embeddingService.GetEmbeddingAsync(chunk.Text, cancellationToken);

                    if (vectorArray == null || vectorArray.Length == 0)
                    {
                        throw new InvalidOperationException($"Embedding generation returned an empty vector for chunk {chunk.Id}.");
                    }

                    var embedding = new PaperFullTextChunkEmbedding
                    {
                        Id = Guid.NewGuid(),
                        ChunkId = chunk.Id,
                        ModelName = modelName,
                        Vector = new Vector(vectorArray),
                        EmbeddedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    };

                    await _unitOfWork.PaperFullTextChunkEmbeddings.AddAsync(embedding);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate embedding for chunk {ChunkId}.", chunk.Id);
                    // As per requirement, fail the whole process if one chunk fails
                    throw;
                }
            }

            // Commit embeddings first
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully inserted new embeddings via repository.");

            // PHASE 5: Update the parent status (ModifiedAt, EmbeddedAt) separately
            refreshedFullText.EmbeddedAt = DateTimeOffset.UtcNow;
            refreshedFullText.ModifiedAt = DateTimeOffset.UtcNow;

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully updated final status for PaperFullText {Id}. PaperPdf={PdfId}", refreshedFullText.Id, paperPdfId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict during final status update for PaperFullText {Id}. The embeddings are likely inserted but the parent status marker failed.", refreshedFullText.Id);
                throw;
            }
        }
    }
}
