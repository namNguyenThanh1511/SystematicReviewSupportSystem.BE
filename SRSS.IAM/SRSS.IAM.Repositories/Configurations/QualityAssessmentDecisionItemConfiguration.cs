using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class QualityAssessmentDecisionItemConfiguration : IEntityTypeConfiguration<QualityAssessmentDecisionItem>
    {
        public void Configure(EntityTypeBuilder<QualityAssessmentDecisionItem> builder)
        {
            builder.ToTable("quality_assessment_decision_items");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(d => d.QualityAssessmentDecisionId)
                .HasColumnName("quality_assessment_decision_id")
                .IsRequired();

            builder.Property(d => d.QualityCriterionId)
                .HasColumnName("quality_criterion_id")
                .IsRequired();

            builder.Property(d => d.Value)
                .HasColumnName("value");

            builder.Property(d => d.Comment)
                .HasColumnName("comment");

            builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at");

            // Relationships
            builder.HasOne(d => d.QualityAssessmentDecision)
                .WithMany(d => d.DecisionItems)
                .HasForeignKey(d => d.QualityAssessmentDecisionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.QualityCriterion)
                .WithMany()
                .HasForeignKey(d => d.QualityCriterionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}