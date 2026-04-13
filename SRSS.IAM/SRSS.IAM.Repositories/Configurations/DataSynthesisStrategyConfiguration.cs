using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class DataSynthesisStrategyConfiguration : IEntityTypeConfiguration<DataSynthesisStrategy>
	{
		public void Configure(EntityTypeBuilder<DataSynthesisStrategy> builder)
		{
			builder.ToTable("data_synthesis_strategy");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("synthesis_strategy_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.SynthesisType)
				.HasColumnName("synthesis_type")
				.IsRequired()
				.HasConversion<string>();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.Property(x => x.TargetResearchQuestionIds)
				.HasColumnName("target_research_question_ids");

			builder.Property(x => x.DataGroupingPlan)
				.HasColumnName("data_grouping_plan");

			builder.Property(x => x.SensitivityAnalysisPlan)
				.HasColumnName("sensitivity_analysis_plan");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.SynthesisStrategies)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}