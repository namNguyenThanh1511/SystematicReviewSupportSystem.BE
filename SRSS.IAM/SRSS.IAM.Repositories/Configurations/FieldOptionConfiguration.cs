using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class FieldOptionConfiguration : IEntityTypeConfiguration<FieldOption>
	{
		public void Configure(EntityTypeBuilder<FieldOption> builder)
		{
			builder.ToTable("field_options");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).HasColumnName("option_id");

			builder.Property(x => x.FieldId).HasColumnName("field_id").IsRequired();
			builder.Property(x => x.Value).HasColumnName("value").IsRequired();
			builder.Property(x => x.DisplayOrder).HasColumnName("display_order");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

			// Index
			builder.HasIndex(x => x.FieldId);
		}
	}
}