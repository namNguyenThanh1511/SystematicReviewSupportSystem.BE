using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.RagService
{
    public interface IRagRetrievalService
    {
        /// <summary>
        /// Retrieves the most semantically relevant chunks for a given search query within a specific paper.
        /// </summary>
        Task<List<PaperChunk>> GetRelevantChunksAsync(Guid paperId, string searchQuestion, int topK = 5, CancellationToken cancellationToken = default);
    }
}
