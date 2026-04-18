using Pgvector;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents a semantic chunk extracted from a Paper's full-text PDF.
    /// Used as the retrieval unit for the RAG (Retrieval-Augmented Generation) pipeline.
    /// </summary>
    public class PaperChunk
    {
        /// <summary>Primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Foreign key to the parent <see cref="Paper"/>.</summary>
        public Guid PaperId { get; set; }

        /// <summary>Navigation property to the parent paper.</summary>
        public Paper Paper { get; set; } = null!;

        /// <summary>Raw text content of this chunk (paragraph or heading from the full text).</summary>
        public string TextContent { get; set; } = string.Empty;

        /// <summary>
        /// JSON string containing Grobid bounding-box coordinates for the source text
        /// (the raw "coords" attribute value from the TEI XML, e.g. "5,72,467,88,12").
        /// </summary>
        public string? CoordinatesJson { get; set; }

        /// <summary>
        /// Embedding vector stored as a PostgreSQL <c>vector</c> column.
        /// The number of dimensions is defined by <see cref="EmbeddingDimensions"/>.
        /// </summary>
        public Vector? Embedding { get; set; }

        // ============================================
        // EMBEDDING MODEL PROVENANCE
        // ============================================

        /// <summary>
        /// The identifier of the embedding model that produced <see cref="Embedding"/>.
        /// Example: "all-MiniLM-L6-v2". Used to detect stale chunks when the model changes.
        /// </summary>
        public string EmbeddingModel { get; set; } = string.Empty;

        /// <summary>
        /// The number of dimensions of the stored embedding vector.
        /// Example: 384 for all-MiniLM-L6-v2.
        /// </summary>
        public int EmbeddingDimensions { get; set; }

        /// <summary>
        /// The library or external service that produced the embedding.
        /// Example: "SmartComponents.LocalEmbeddings", "OpenAI", "Google".
        /// </summary>
        public string EmbeddingProvider { get; set; } = string.Empty;

        /// <summary>UTC timestamp when this chunk was created.</summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
