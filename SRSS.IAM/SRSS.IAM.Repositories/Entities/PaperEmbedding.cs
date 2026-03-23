using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperEmbedding : BaseEntity<Guid>
    {
        public Guid PaperId { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public string Model { get; set; } = "text-embedding-3-small";

        // Navigation property
        public Paper Paper { get; set; } = null!;
    }
}
