using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperPdfConfiguration : IEntityTypeConfiguration<PaperPdf>
    {
        public void Configure(EntityTypeBuilder<PaperPdf> builder)
        {
            builder.ToTable("paper_pdfs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath).IsRequired();
            builder.Property(x => x.FileName).IsRequired();

            builder.HasOne(x => x.Paper)
                   .WithMany(p => p.PaperPdfs)
                   .HasForeignKey(x => x.PaperId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
