using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class QualityAssessmentProcessConfiguration : IEntityTypeConfiguration<QualityAssessmentProcess>
    {
        public void Configure(EntityTypeBuilder<QualityAssessmentProcess> builder)
        {
            builder.ToTable("quality_assessment_processes");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(q => q.ReviewProcessId)
                .HasColumnName("review_process_id")
                .IsRequired();

            builder.Property(q => q.Notes)
                .HasColumnName("notes");

            builder.Property(q => q.Status)
               .HasColumnName("status")
               .IsRequired();

            builder.Property(q => q.StartedAt)
                .HasColumnName("started_at");

            builder.Property(q => q.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

            builder.Property(u => u.ModifiedAt)
                .HasColumnName("modified_at");

            // Relationships
            builder.HasOne(q => q.ReviewProcess)
                .WithOne(r => r.QualityAssessmentProcess)
                .HasForeignKey<QualityAssessmentProcess>(q => q.ReviewProcessId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
