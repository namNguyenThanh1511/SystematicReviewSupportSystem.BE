using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperFullTextParsedParagraphConfiguration : IEntityTypeConfiguration<PaperFullTextParsedParagraph>
    {
        public void Configure(EntityTypeBuilder<PaperFullTextParsedParagraph> builder)
        {
            builder.ToTable("paper_full_text_parsed_paragraphs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Text).IsRequired();
            builder.Property(x => x.Order).IsRequired();

            // Index/Unique constraint on (SectionId, Order)
            builder.HasIndex(x => new { x.SectionId, x.Order })
                   .IsUnique()
                   .HasDatabaseName("idx_paper_full_text_parsed_paragraphs_section_id_order");

            // Relationship with Section
            builder.HasOne(x => x.Section)
                   .WithMany(s => s.Paragraphs)
                   .HasForeignKey(x => x.SectionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
