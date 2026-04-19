using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperFullTextChunk : BaseEntity<Guid>
    {
        public Guid PaperFullTextId { get; set; }
        public int Order { get; set; }
        public string SectionTitle { get; set; } = string.Empty;
        public string? SectionType { get; set; }
        public string Text { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public virtual PaperFullTextChunkEmbedding? Embedding { get; set; }

        public virtual PaperFullText? PaperFullText { get; set; }
    }
}
