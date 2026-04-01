using Pgvector;

namespace SRSS.IAM.Services.RagService
{
    /// <summary>
    /// Provides CPU-local text embeddings using <c>SmartComponents.LocalEmbeddings</c>.
    /// Embeddings are 384-dimensional (all-MiniLM-L6-v2 compatible) and returned as
    /// <see cref="Vector"/> objects ready for storage in a PostgreSQL <c>vector(384)</c> column.
    /// </summary>
    public interface ILocalEmbeddingService
    {
        /// <summary>
        /// Generates a 384-dimensional embedding for a single text string.
        /// </summary>
        /// <param name="text">The text to embed. Must not be null or empty.</param>
        /// <returns>A <see cref="Vector"/> with 384 dimensions.</returns>
        Vector GetEmbedding(string text);

        /// <summary>
        /// Generates embeddings for a batch of text strings.
        /// </summary>
        /// <param name="texts">List of texts to embed.</param>
        /// <returns>A list of <see cref="Vector"/> objects (same order as input).</returns>
        List<Vector> GetEmbeddingsBatch(List<string> texts);
    }
}
