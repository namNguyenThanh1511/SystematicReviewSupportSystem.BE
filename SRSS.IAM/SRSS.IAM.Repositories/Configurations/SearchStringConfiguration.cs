using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class SearchStringConfiguration : IEntityTypeConfiguration<SearchString>
	{
		public void Configure(EntityTypeBuilder<SearchString> builder)
		{
			builder.ToTable("search_string");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("search_string_id")
				.IsRequired();

			builder.Property(x => x.StrategyId)
				.HasColumnName("strategy_id")
				.IsRequired();

			builder.Property(x => x.Expression)
				.HasColumnName("expression")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Strategy)
				.WithMany(x => x.SearchStrings)
				.HasForeignKey(x => x.StrategyId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}