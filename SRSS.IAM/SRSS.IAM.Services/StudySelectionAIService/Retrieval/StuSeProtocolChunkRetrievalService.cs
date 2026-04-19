using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.DTOs.PaperFullText;
using SRSS.IAM.Services.DTOs.StudySelection;
using SRSS.IAM.Services.PaperFullTextService.Search;

namespace SRSS.IAM.Services.StudySelectionAIService.Retrieval
{
    public interface IStuSeProtocolChunkRetrievalService
    {
        Task<List<ChunkSearchResultDto>> RetrieveRelevantChunksAsync(
            Guid paperPdfId,
            StuSeAIInput input,
            int topKPerQuery,
            CancellationToken cancellationToken = default);
    }

    public class StuSeProtocolChunkRetrievalService : IStuSeProtocolChunkRetrievalService
    {
        private readonly IStuSeProtocolRetrievalQueryBuilder _queryBuilder;
        private readonly IPaperChunkSemanticSearchService _semanticSearchService;
        private readonly ILogger<StuSeProtocolChunkRetrievalService> _logger;

        public StuSeProtocolChunkRetrievalService(
            IStuSeProtocolRetrievalQueryBuilder queryBuilder,
            IPaperChunkSemanticSearchService semanticSearchService,
            ILogger<StuSeProtocolChunkRetrievalService> logger)
        {
            _queryBuilder = queryBuilder;
            _semanticSearchService = semanticSearchService;
            _logger = logger;
        }

        public async Task<List<ChunkSearchResultDto>> RetrieveRelevantChunksAsync(
            Guid paperPdfId,
            StuSeAIInput input,
            int topKPerQuery,
            CancellationToken cancellationToken = default)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (topKPerQuery <= 0)
                throw new ArgumentOutOfRangeException(nameof(topKPerQuery), "topKPerQuery must be greater than 0.");


            _logger.LogInformation("Starting protocol-driven chunk retrieval for PaperPdf {PaperPdfId}", paperPdfId);

            // 1. Generate queries from protocol input
            var queries = _queryBuilder.BuildQueries(input);
            _logger.LogInformation("Generated {Count} semantic retrieval queries from protocol input.", queries.Count);

            if (queries.Count == 0)
            {
                throw new InvalidOperationException("No retrieval queries could be generated from the given protocol input.");
            }

            // 2. Execute semantic search for each query
            var allResults = new List<ChunkSearchResultDto>();

            foreach (var query in queries)
            {
                _logger.LogDebug("Executing {QueryType} query: {QueryText}", query.QueryType, query.QueryText);

                var chunks = await _semanticSearchService.SearchRelevantChunksAsync(
                    paperPdfId,
                    query.QueryText,
                    topKPerQuery,
                    cancellationToken);

                _logger.LogDebug("Query '{QueryType}' returned {Count} chunks.", query.QueryType, chunks.Count);
                allResults.AddRange(chunks);
            }

            // 3. Merge and deduplicate
            // Keep the one with the highest similarity score if duplicate ChunkId exists
            var mergedResults = allResults
                .GroupBy(c => c.ChunkId)
                .Select(group => group.OrderByDescending(c => c.SimilarityScore).First())
                .OrderByDescending(c => c.SimilarityScore)
                .ToList();

            _logger.LogInformation("Protocol retrieval completed for PaperPdf {PaperPdfId}. " +
                "TotalRawHits: {RawTotal}, UniqueChunks: {UniqueCount}",
                paperPdfId, allResults.Count, mergedResults.Count);

            return mergedResults;
        }
    }
}
