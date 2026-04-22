using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperFullTextChunkEmbeddingConfiguration : IEntityTypeConfiguration<PaperFullTextChunkEmbedding>
    {
        public void Configure(EntityTypeBuilder<PaperFullTextChunkEmbedding> builder)
        {
            builder.ToTable("paper_full_text_chunk_embeddings");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Vector).HasColumnType("vector");
            builder.Property(x => x.ModelName).IsRequired().HasMaxLength(100);

            builder.HasOne(x => x.Chunk)
                   .WithOne(c => c.Embedding)
                   .HasForeignKey<PaperFullTextChunkEmbedding>(x => x.ChunkId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
