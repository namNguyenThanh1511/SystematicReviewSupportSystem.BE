using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector.EntityFrameworkCore;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.RagService
{
    public class RagRetrievalService : IRagRetrievalService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalEmbeddingService _embeddingService;
        private readonly ILogger<RagRetrievalService> _logger;

        public RagRetrievalService(
            AppDbContext dbContext,
            ILocalEmbeddingService embeddingService,
            ILogger<RagRetrievalService> logger)
        {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
            _logger = logger;
        }

        public async Task<List<PaperChunk>> GetRelevantChunksAsync(Guid paperId, string searchQuestion, int topK = 5, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchQuestion))
                return new List<PaperChunk>();

            try
            {
                // 1. Vectorize the search query using the same embedding model
                var queryVector = _embeddingService.GetEmbedding(searchQuestion);

                // 2. Query Postgres using pgvector's CosineDistance operator
                var results = await _dbContext.PaperChunks
                    .Where(c => c.PaperId == paperId)
                    .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
                    .Take(topK)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation($"RagRetrievalService retrieved {results.Count} chunks for PaperId {paperId}");
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving relevant chunks for PaperId {paperId}: {ex.Message}");
                return new List<PaperChunk>();
            }
        }
    }
}
