using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class QualityAssessmentAssignmentConfiguration : IEntityTypeConfiguration<QualityAssessmentAssignment>
    {
        public void Configure(EntityTypeBuilder<QualityAssessmentAssignment> builder)
        {
            builder.ToTable("quality_assessment_assignments");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.QualityAssessmentProcessId)
                .HasColumnName("quality_assessment_process_id")
                .IsRequired();

            builder.Property(q => q.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(q => q.AssignedAt)
                .HasColumnName("assigned_at")
                .IsRequired();

            builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at");

            // Relationships
            builder.HasOne(q => q.QualityAssessmentProcess)
                .WithMany(p => p.QualityAssessmentAssignments)
                .HasForeignKey(q => q.QualityAssessmentProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(q => q.User)
                .WithMany()
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(q => q.Papers)
                .WithMany(p => p.QualityAssessmentAssignments)
                .UsingEntity<Dictionary<string, object>>(
                    "quality_assessment_assignment_papers",
                    j => j
                        .HasOne<Paper>()
                        .WithMany()
                        .HasForeignKey("paper_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<QualityAssessmentAssignment>()
                        .WithMany()
                        .HasForeignKey("quality_assessment_assignment_id")
                        .OnDelete(DeleteBehavior.Cascade)
                );
        }
    }
}
