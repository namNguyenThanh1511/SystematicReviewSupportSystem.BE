using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class QualityAssessmentResolutionConfiguration : IEntityTypeConfiguration<QualityAssessmentResolution>
    {
        public void Configure(EntityTypeBuilder<QualityAssessmentResolution> builder)
        {
            builder.ToTable("quality_assessment_resolutions");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(r => r.QualityAssessmentProcessId)
                .HasColumnName("quality_assessment_process_id")
                .IsRequired();

            builder.Property(r => r.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(r => r.ResolvedBy)
                .HasColumnName("resolved_by")
                .IsRequired();

            builder.Property(r => r.FinalDecision)
                .HasColumnName("final_decision")
                .IsRequired();

            builder.Property(r => r.FinalScore)
                .HasColumnName("final_score");

            builder.Property(r => r.ResolutionNotes)
                .HasColumnName("resolution_notes");

            builder.Property(r => r.ResolvedAt)
                .HasColumnName("resolved_at")
                .IsRequired();

            builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

            // Relationships
            builder.HasOne(r => r.QualityAssessmentProcess)
                .WithMany(p => p.QualityAssessmentResolutions)
                .HasForeignKey(r => r.QualityAssessmentProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Paper)
                .WithMany()
                .HasForeignKey(r => r.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.ResolvedByUser)
                .WithMany()
                .HasForeignKey(r => r.ResolvedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
