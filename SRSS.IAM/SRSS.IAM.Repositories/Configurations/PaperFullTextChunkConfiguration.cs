using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperFullTextChunkConfiguration : IEntityTypeConfiguration<PaperFullTextChunk>
    {
        public void Configure(EntityTypeBuilder<PaperFullTextChunk> builder)
        {
            builder.ToTable("paper_full_text_chunks");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text).IsRequired();
            builder.Property(x => x.Order).IsRequired();
            builder.Property(x => x.SectionTitle).IsRequired();

            // Index for ordering and filtering
            builder.HasIndex(x => new { x.PaperFullTextId, x.Order })
                   .IsUnique()
                   .HasDatabaseName("idx_paper_full_text_chunks_paper_id_order");

            // Relationship with PaperFullText
            builder.HasOne(x => x.PaperFullText)
                   .WithMany(p => p.Chunks)
                   .HasForeignKey(x => x.PaperFullTextId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
