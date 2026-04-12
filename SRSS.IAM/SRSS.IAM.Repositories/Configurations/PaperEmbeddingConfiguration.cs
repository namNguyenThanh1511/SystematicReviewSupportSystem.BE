using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperEmbeddingConfiguration : IEntityTypeConfiguration<PaperEmbedding>
    {
        public void Configure(EntityTypeBuilder<PaperEmbedding> builder)
        {
            builder.ToTable("paper_embeddings");

            builder.HasKey(pe => pe.Id);

            builder.Property(pe => pe.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(pe => pe.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(pe => pe.Embedding)
                .HasColumnName("embedding")
                .HasColumnType("vector")
                .IsRequired();

            builder.Property(pe => pe.Model)
                .HasColumnName("model")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(pe => pe.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(pe => pe.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // 1:1 Relationship with Paper
            builder.HasOne(pe => pe.Paper)
                .WithOne(p => p.TitleEmbedding)
                .HasForeignKey<PaperEmbedding>(pe => pe.PaperId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
