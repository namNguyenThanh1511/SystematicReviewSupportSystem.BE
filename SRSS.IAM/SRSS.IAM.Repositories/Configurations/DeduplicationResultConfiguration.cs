using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class DeduplicationResultConfiguration : IEntityTypeConfiguration<DeduplicationResult>
    {
        public void Configure(EntityTypeBuilder<DeduplicationResult> builder)
        {
            builder.ToTable("deduplication_results");

            builder.HasKey(dr => dr.Id);

            builder.Property(dr => dr.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(dr => dr.IdentificationProcessId)
                .HasColumnName("identification_process_id")
                .IsRequired();

            builder.Property(dr => dr.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(dr => dr.DuplicateOfPaperId)
                .HasColumnName("duplicate_of_paper_id")
                .IsRequired();

            builder.Property(dr => dr.Method)
                .HasColumnName("method")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(dr => dr.ConfidenceScore)
                .HasColumnName("confidence_score")
                .HasPrecision(5, 4); // e.g., 0.9876

            builder.Property(dr => dr.Notes)
                .HasColumnName("notes");

            builder.Property(dr => dr.ReviewStatus)
                .HasColumnName("review_status")
                .HasConversion<string>()
                .HasDefaultValue(DeduplicationReviewStatus.Pending)
                .IsRequired();

            builder.Property(dr => dr.ReviewedBy)
                .HasColumnName("reviewed_by");

            builder.Property(dr => dr.ReviewedAt)
                .HasColumnName("reviewed_at");

            builder.Property(dr => dr.ResolvedDecision)
                .HasColumnName("resolved_decision")
                .HasMaxLength(50);

            builder.Property(dr => dr.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(dr => dr.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(dr => dr.IdentificationProcess)
                .WithMany(ip => ip.DeduplicationResults)
                .HasForeignKey(dr => dr.IdentificationProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dr => dr.Paper)
                .WithMany(p => p.DuplicateResults)
                .HasForeignKey(dr => dr.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dr => dr.DuplicateOfPaper)
                .WithMany(p => p.OriginalOfDuplicates)
                .HasForeignKey(dr => dr.DuplicateOfPaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: One deduplication result per paper per process
            builder.HasIndex(dr => new { dr.IdentificationProcessId, dr.PaperId })
                .IsUnique()
                .HasDatabaseName("uq_deduplication_process_paper");

            // Check constraint: Prevent self-duplicate
            builder.HasCheckConstraint(
                "ck_deduplication_no_self_duplicate",
                "paper_id != duplicate_of_paper_id");

            // Additional indexes for performance
            builder.HasIndex(dr => dr.IdentificationProcessId);
            builder.HasIndex(dr => dr.PaperId);
            builder.HasIndex(dr => dr.DuplicateOfPaperId);
            builder.HasIndex(dr => dr.Method);
            builder.HasIndex(dr => dr.ReviewStatus);
        }
    }
}
