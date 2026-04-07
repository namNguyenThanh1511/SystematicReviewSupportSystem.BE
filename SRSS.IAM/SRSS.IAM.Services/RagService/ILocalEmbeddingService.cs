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
        // ============================================
        // MODEL IDENTITY (for chunk provenance tracking)
        // ============================================

        /// <summary>
        /// Short model identifier stored on every <c>PaperChunk</c>.
        /// Example: "all-MiniLM-L6-v2".
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Number of dimensions in every embedding vector produced by this service.
        /// Example: 384.
        /// </summary>
        int Dimensions { get; }

        /// <summary>
        /// Library or external service backing this implementation.
        /// Example: "SmartComponents.LocalEmbeddings".
        /// </summary>
        string Provider { get; }

        // ============================================
        // EMBEDDING METHODS
        // ============================================

        /// <summary>
        /// Generates a single embedding for the provided text.
        /// </summary>
        /// <param name="text">The text to embed. Must not be null or empty.</param>
        /// <returns>A <see cref="Vector"/> with <see cref="Dimensions"/> dimensions.</returns>
        Vector GetEmbedding(string text);

        /// <summary>
        /// Generates embeddings for a batch of text strings in a single pass.
        /// </summary>
        /// <param name="texts">List of texts to embed.</param>
        /// <returns>A list of <see cref="Vector"/> objects in the same order as <paramref name="texts"/>.</returns>
        List<Vector> GetEmbeddingsBatch(List<string> texts);
    }
}
