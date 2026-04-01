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
        /// 384-dimensional embedding vector produced by <c>SmartComponents.LocalEmbeddings</c>.
        /// Stored as PostgreSQL <c>vector(384)</c>.
        /// </summary>
        public Vector? Embedding { get; set; }

        /// <summary>UTC timestamp when this chunk was created.</summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
