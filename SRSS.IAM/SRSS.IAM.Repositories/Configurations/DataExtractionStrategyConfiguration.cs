using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class DataExtractionStrategyConfiguration : IEntityTypeConfiguration<DataExtractionStrategy>
	{
		public void Configure(EntityTypeBuilder<DataExtractionStrategy> builder)
		{
			builder.ToTable("data_extraction_strategy");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("extraction_strategy_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.ExtractionStrategies)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}