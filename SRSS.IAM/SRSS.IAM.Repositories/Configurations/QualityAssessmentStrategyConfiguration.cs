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
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.QualityStrategies)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}