using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.PaperFullText;
using SRSS.IAM.Services.EmbeddingService;

namespace SRSS.IAM.Services.PaperFullTextService.Search
{
    public interface IPaperChunkSemanticSearchService
    {
        Task<List<ChunkSearchResultDto>> SearchRelevantChunksAsync(
            Guid paperPdfId,
            string query,
            int topK,
            CancellationToken cancellationToken = default);
    }

    public class PaperChunkSemanticSearchService : IPaperChunkSemanticSearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<PaperChunkSemanticSearchService> _logger;

        public PaperChunkSemanticSearchService(
            IUnitOfWork unitOfWork,
            IEmbeddingService embeddingService,
            ILogger<PaperChunkSemanticSearchService> logger)
        {
            _unitOfWork = unitOfWork;
            _embeddingService = embeddingService;
            _logger = logger;
        }

        public async Task<List<ChunkSearchResultDto>> SearchRelevantChunksAsync(
            Guid paperPdfId,
            string query,
            int topK,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Performing semantic search on PaperPdf {PaperPdfId} with query: '{Query}'", paperPdfId, query);

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Search query cannot be empty.", nameof(query));
            }

            // 1. Load full-text, chunks, and embeddings
            var fullText = await _unitOfWork.PaperFullTexts.GetQueryable(ft => ft.PaperPdfId == paperPdfId, isTracking: false)
                .Include(ft => ft.Chunks)
                    .ThenInclude(c => c.Embedding)
                .FirstOrDefaultAsync(cancellationToken);

            if (fullText == null)
            {
                throw new InvalidOperationException($"No PaperFullText found for PaperPdf {paperPdfId}.");
            }

            if (!fullText.Chunks.Any())
            {
                throw new InvalidOperationException($"No chunks found for PaperFullText {fullText.Id}. Chunking must be completed first.");
            }

            if (fullText.Chunks.All(c => c.Embedding == null))
            {
                throw new InvalidOperationException($"No embeddings found for chunks of PaperFullText {fullText.Id}. Embedding must be completed first.");
            }

            // 2. Generate embedding for query
            var queryVector = await _embeddingService.GetEmbeddingAsync(query, cancellationToken);
            if (queryVector == null || queryVector.Length == 0)
            {
                throw new InvalidOperationException("Failed to generate embedding for the search query.");
            }

            // 3. Compute similarity and sort
            var results = fullText.Chunks
                .Where(c => c.Embedding != null)
                .Select(c =>
                {
                    var chunkVector = c.Embedding!.Vector.ToArray();
                    var score = ComputeCosineSimilarity(queryVector, chunkVector);
                    
                    return new ChunkSearchResultDto
                    {
                        ChunkId = c.Id,
                        Order = c.Order,
                        SectionTitle = c.SectionTitle,
                        SectionType = c.SectionType,
                        Text = c.Text,
                        SimilarityScore = score
                    };
                })
                .OrderByDescending(r => r.SimilarityScore)
                .Take(topK)
                .ToList();

            _logger.LogInformation("Semantic search completed. Found {Count} matching chunks.", results.Count);

            return results;
        }

        private double ComputeCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
            {
                throw new ArgumentException("Vectors must have the same length.");
            }

            double dotProduct = 0;
            double normA = 0;
            double normB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }

            if (normA == 0 || normB == 0) return 0;

            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
