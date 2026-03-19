using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Configurations
{
    public class IdentificationProcessPaperConfiguration : IEntityTypeConfiguration<IdentificationProcessPaper>
    {
        public void Configure(EntityTypeBuilder<IdentificationProcessPaper> builder)
        {
            builder.ToTable("identification_process_papers");

            builder.HasKey(ipp => ipp.Id);

            builder.Property(ipp => ipp.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ipp => ipp.IdentificationProcessId)
                .HasColumnName("identification_process_id")
                .IsRequired();

            builder.Property(ipp => ipp.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(ipp => ipp.IncludedAfterDedup)
                .HasColumnName("included_after_dedup")
                .IsRequired();

            builder.Property(ipp => ipp.SourceType)
                .HasColumnName("source_type")
                .IsRequired()
                .HasDefaultValue(PaperSourceType.DatabaseSearch);

            builder.Property(ipp => ipp.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ipp => ipp.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(ipp => ipp.IdentificationProcess)
                .WithMany(ip => ip.IdentificationPapers)
                .HasForeignKey(ipp => ipp.IdentificationProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ipp => ipp.Paper)
                .WithMany(p => p.IdentificationProcessPapers)
                .HasForeignKey(ipp => ipp.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: One entry per paper per identification process
            builder.HasIndex(ipp => new { ipp.IdentificationProcessId, ipp.PaperId })
                .IsUnique()
                .HasDatabaseName("uq_identification_process_paper");

            // Additional indexes for performance
            builder.HasIndex(ipp => ipp.IdentificationProcessId);
            builder.HasIndex(ipp => ipp.PaperId);
            builder.HasIndex(ipp => ipp.IncludedAfterDedup);
        }
    }
}
