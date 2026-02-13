using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class DataItemDefinitionConfiguration : IEntityTypeConfiguration<DataItemDefinition>
	{
		public void Configure(EntityTypeBuilder<DataItemDefinition> builder)
		{
			builder.ToTable("data_item_definition");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("data_item_id")
				.IsRequired();

			builder.Property(x => x.FormId)
				.HasColumnName("form_id")
				.IsRequired();

			builder.Property(x => x.Name)
				.HasColumnName("name")
				.IsRequired();

			builder.Property(x => x.DataType)
				.HasColumnName("data_type")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.HasOne(x => x.Form)
				.WithMany(x => x.DataItems)
				.HasForeignKey(x => x.FormId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}