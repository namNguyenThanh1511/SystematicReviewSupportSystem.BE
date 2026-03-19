using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class CandidatePaperConfiguration : IEntityTypeConfiguration<CandidatePaper>
    {
        public void Configure(EntityTypeBuilder<CandidatePaper> builder)
        {
            builder.ToTable("CandidatePapers");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.Authors).HasMaxLength(2000);
            builder.Property(c => c.PublicationYear).HasMaxLength(50);
            builder.Property(c => c.DOI).HasMaxLength(255);
            
            builder.HasOne(c => c.ReviewProcess)
                .WithMany()
                .HasForeignKey(c => c.ReviewProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.OriginPaper)
                .WithMany()
                .HasForeignKey(c => c.OriginPaperId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
