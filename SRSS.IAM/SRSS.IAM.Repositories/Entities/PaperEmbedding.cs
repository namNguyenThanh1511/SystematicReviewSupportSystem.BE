using Shared.Entities.BaseEntity;
using Pgvector;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperEmbedding : BaseEntity<Guid>
    {
        public Guid PaperId { get; set; }
        public Vector Embedding { get; set; } = null!;
        public string Model { get; set; } = "text-embedding-3-small";

        // Navigation property
        public Paper Paper { get; set; } = null!;
    }
}
