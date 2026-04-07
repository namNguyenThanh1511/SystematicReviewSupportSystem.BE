using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperChunkConfiguration : IEntityTypeConfiguration<PaperChunk>
    {
        public void Configure(EntityTypeBuilder<PaperChunk> builder)
        {
            builder.ToTable("paper_chunks");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(c => c.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(c => c.TextContent)
                .HasColumnName("text_content")
                .IsRequired();

            builder.Property(c => c.CoordinatesJson)
                .HasColumnName("coordinates_json");

            // pgvector column: 384-dimensional float vector (SmartComponents.LocalEmbeddings output)
            builder.Property(c => c.Embedding)
                .HasColumnName("embedding")
                .HasColumnType("vector(384)");

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(c => c.EmbeddingModel)
                .HasColumnName("embedding_model")
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(c => c.EmbeddingDimensions)
                .HasColumnName("embedding_dimensions")
                .IsRequired();

            builder.Property(c => c.EmbeddingProvider)
                .HasColumnName("embedding_provider")
                .HasMaxLength(128)
                .IsRequired();

            // Relationship: many PaperChunks → one Paper (cascade delete)
            builder.HasOne(c => c.Paper)
                .WithMany(p => p.PaperChunks)
                .HasForeignKey(c => c.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // IVFFlat index for approximate nearest-neighbour cosine search.
            // lists=100 is a reasonable default; tune based on total row count.
            builder.HasIndex(c => c.Embedding)
                .HasMethod("ivfflat")
                .HasOperators("vector_cosine_ops")
                .HasStorageParameter("lists", 100);
        }
    }
}
