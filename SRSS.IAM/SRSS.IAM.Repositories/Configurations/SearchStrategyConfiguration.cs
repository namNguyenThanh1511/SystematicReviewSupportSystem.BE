using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class SearchStrategyConfiguration : IEntityTypeConfiguration<SearchStrategy>
	{
		public void Configure(EntityTypeBuilder<SearchStrategy> builder)
		{
			builder.ToTable("search_strategy");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("strategy_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.SearchStrategies)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
