using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperFullTextConfiguration : IEntityTypeConfiguration<PaperFullText>
    {
        public void Configure(EntityTypeBuilder<PaperFullText> builder)
        {
            builder.ToTable("paper_full_texts");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RawXml).IsRequired();

            // 1-to-1 relationship with PaperPdf
            builder.HasOne(x => x.PaperPdf)
                   .WithOne(p => p.PaperFullText)
                   .HasForeignKey<PaperFullText>(x => x.PaperPdfId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
