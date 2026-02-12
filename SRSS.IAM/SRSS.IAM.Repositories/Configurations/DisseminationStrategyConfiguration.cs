using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class DisseminationStrategyConfiguration : IEntityTypeConfiguration<DisseminationStrategy>
	{
		public void Configure(EntityTypeBuilder<DisseminationStrategy> builder)
		{
			builder.ToTable("dissemination_strategy");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("dissemination_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Channel)
				.HasColumnName("channel")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.DisseminationStrategies)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}