using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperFullTextParsedSectionConfiguration : IEntityTypeConfiguration<PaperFullTextParsedSection>
    {
        public void Configure(EntityTypeBuilder<PaperFullTextParsedSection> builder)
        {
            builder.ToTable("paper_full_text_parsed_sections");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SectionTitle).IsRequired();
            builder.Property(x => x.Order).IsRequired();

            // Index/Unique constraint on (PaperFullTextId, Order)
            builder.HasIndex(x => new { x.PaperFullTextId, x.Order })
                   .IsUnique()
                   .HasDatabaseName("idx_paper_full_text_parsed_sections_paper_id_order");

            // Relationship with PaperFullText
            builder.HasOne(x => x.PaperFullText)
                   .WithMany(p => p.ParsedSections)
                   .HasForeignKey(x => x.PaperFullTextId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
