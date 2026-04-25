using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class QualityAssessmentDecisionConfiguration : IEntityTypeConfiguration<QualityAssessmentDecision>
    {
        public void Configure(EntityTypeBuilder<QualityAssessmentDecision> builder)
        {
            builder.ToTable("quality_assessment_decisions");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(d => d.QualityAssessmentProcessId)
                .HasColumnName("quality_assessment_process_id")
                .IsRequired();

            builder.Property(d => d.ReviewerId)
                .HasColumnName("reviewer_id")
                .IsRequired();

            builder.Property(d => d.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(d => d.Score)
                .HasColumnName("score")
                .HasColumnType("decimal(5,2)");

            // builder.Property(d => d.Notes)
            //     .HasColumnName("notes");

            builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at");

            // Relationships
            builder.HasOne(d => d.QualityAssessmentProcess)
                .WithMany(p => p.QualityAssessmentDecisions)
                .HasForeignKey(d => d.QualityAssessmentProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Reviewer)
                .WithMany()
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Paper)
                .WithMany(p => p.QualityAssessmentDecisions)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.DecisionItems)
                .WithOne(di => di.QualityAssessmentDecision)
                .HasForeignKey(di => di.QualityAssessmentDecisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
