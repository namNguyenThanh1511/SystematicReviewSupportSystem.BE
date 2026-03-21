using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class GrobidHeaderResultConfiguration : IEntityTypeConfiguration<GrobidHeaderResult>
    {
        public void Configure(EntityTypeBuilder<GrobidHeaderResult> builder)
        {
            builder.ToTable("grobid_header_results");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.PaperPdf)
                   .WithOne(p => p.GrobidHeaderResult)
                   .HasForeignKey<GrobidHeaderResult>(x => x.PaperPdfId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
