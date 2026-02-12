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
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.SynthesisType)
				.HasColumnName("synthesis_type")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.SynthesisStrategies)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}