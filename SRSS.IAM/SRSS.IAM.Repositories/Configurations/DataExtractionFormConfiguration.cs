using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class DataExtractionFormConfiguration : IEntityTypeConfiguration<DataExtractionForm>
	{
		public void Configure(EntityTypeBuilder<DataExtractionForm> builder)
		{
			builder.ToTable("data_extraction_form");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("form_id")
				.IsRequired();

			builder.Property(x => x.ExtractionStrategyId)
				.HasColumnName("extraction_strategy_id")
				.IsRequired();

			builder.Property(x => x.Name)
				.HasColumnName("name")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.ExtractionStrategy)
				.WithMany(x => x.Forms)
				.HasForeignKey(x => x.ExtractionStrategyId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}