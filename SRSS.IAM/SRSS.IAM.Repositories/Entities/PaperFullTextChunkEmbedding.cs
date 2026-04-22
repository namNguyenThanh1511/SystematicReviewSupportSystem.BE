using Shared.Entities.BaseEntity;
using Pgvector;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperFullTextChunkEmbedding : BaseEntity<Guid>
    {
        public Guid ChunkId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public Vector Vector { get; set; } = null!;
        public DateTimeOffset EmbeddedAt { get; set; }

        public virtual PaperFullTextChunk? Chunk { get; set; }
    }
}
