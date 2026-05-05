using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

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
         builder.Property(x => x.FileHash).HasMaxLength(128);
         builder.Property(x => x.ExtractedDoi).HasMaxLength(255);

         builder.Property(x => x.ValidationStatus)
             .HasDefaultValue(PdfValidationStatus.Pending);
            builder.Property(x => x.ValidationStatus)
                .HasConversion<string>();

         builder.Property(x => x.ProcessingStatus)
             .HasDefaultValue(PdfProcessingStatus.Uploaded);
            builder.Property(x => x.ProcessingStatus)
                .HasConversion<string>();

         builder.Property(x => x.MetadataProcessedAt);
         builder.Property(x => x.MetadataValidatedAt);
         builder.Property(x => x.FullTextProcessedAt);
         builder.Property(x => x.PageWidth);
         builder.Property(x => x.PageHeight);

            builder.HasOne(x => x.Paper)
                   .WithMany(p => p.PaperPdfs)
                   .HasForeignKey(x => x.PaperId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
