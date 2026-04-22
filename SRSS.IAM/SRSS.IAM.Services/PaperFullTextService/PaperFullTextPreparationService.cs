using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.PaperFullTextService.Chunking;
using SRSS.IAM.Services.PaperFullTextService.Embedding;

namespace SRSS.IAM.Services.PaperFullTextService
{
    public class PaperFullTextPreparationService : IPaperFullTextPreparationService
    {
        private readonly IPaperFullTextService _fullTextService;
        private readonly IPaperFullTextChunkingService _chunkingService;
        private readonly IPaperFullTextChunkEmbeddingService _embeddingService;
        private readonly ILogger<PaperFullTextPreparationService> _logger;

        public PaperFullTextPreparationService(
            IPaperFullTextService fullTextService,
            IPaperFullTextChunkingService chunkingService,
            IPaperFullTextChunkEmbeddingService embeddingService,
            ILogger<PaperFullTextPreparationService> logger)
        {
            _fullTextService = fullTextService;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _logger = logger;
        }

        public async Task PrepareForAiAsync(Guid paperPdfId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Orchestrating AI preparation for PaperPdf {PaperPdfId}", paperPdfId);

            // 1. Parse the full-text
            _logger.LogInformation("Step 1/3: Parsing full-text for PaperPdf {PaperPdfId}", paperPdfId);
            await _fullTextService.ParseFullTextAsync(paperPdfId, cancellationToken);

            // 2. Chunk the full-text
            _logger.LogInformation("Step 2/3: Chunking full-text for PaperPdf {PaperPdfId}", paperPdfId);
            await _chunkingService.ChunkFullTextAsync(paperPdfId, cancellationToken);

            // 3. Generate embeddings for the chunks
            _logger.LogInformation("Step 3/3: Embedding chunks for PaperPdf {PaperPdfId}", paperPdfId);
            await _embeddingService.EmbedChunksAsync(paperPdfId, cancellationToken);

            _logger.LogInformation("AI preparation successfully completed for PaperPdf {PaperPdfId}.", paperPdfId);
        }
    }
}
