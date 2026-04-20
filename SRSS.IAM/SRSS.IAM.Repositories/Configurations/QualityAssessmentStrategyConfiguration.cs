using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class QualityAssessmentStrategyConfiguration : IEntityTypeConfiguration<QualityAssessmentStrategy>
	{
		public void Configure(EntityTypeBuilder<QualityAssessmentStrategy> builder)
		{
			builder.ToTable("quality_assessment_strategy");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("qa_strategy_id")
				.IsRequired();

			builder.Property(x => x.ProjectId)
				.HasColumnName("project_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.HasOne(x => x.Project)
				.WithMany(x => x.QualityStrategies)
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}