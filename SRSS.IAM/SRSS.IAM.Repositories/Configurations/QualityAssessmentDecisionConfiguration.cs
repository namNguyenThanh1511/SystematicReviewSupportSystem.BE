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

            builder.Property(d => d.ReviewerId)
                .HasColumnName("reviewer_id")
                .IsRequired();

            builder.Property(d => d.PaperId)
                .HasColumnName("paper_id")
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
            builder.HasOne(d => d.Reviewer)
                .WithMany()
                .HasForeignKey(d => d.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Paper)
                .WithMany(p => p.QualityAssessmentDecisions)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.QualityCriterion)
                .WithMany()
                .HasForeignKey(d => d.QualityCriterionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
