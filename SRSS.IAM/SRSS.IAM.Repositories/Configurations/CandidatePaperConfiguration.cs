using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class CandidatePaperConfiguration : IEntityTypeConfiguration<CandidatePaper>
    {
        public void Configure(EntityTypeBuilder<CandidatePaper> builder)
        {
            builder.ToTable("candidate_papers");

            builder.HasKey(c => c.Id)
            .HasName("id");

            builder.Property(c => c.Title)
                .HasColumnName("title")
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.Authors)
                .HasColumnName("authors")
                .HasMaxLength(2000);
            builder.Property(c => c.PublicationYear)
                .HasColumnName("publication_year")
                .HasMaxLength(50);
            builder.Property(c => c.DOI)
                .HasColumnName("doi")
                .HasMaxLength(255);
            builder.Property(c => c.ConfidenceScore)
                .HasColumnName("confidence_score");
            builder.HasOne(c => c.OriginPaper)
                .WithMany()
                .HasForeignKey(c => c.OriginPaperId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
