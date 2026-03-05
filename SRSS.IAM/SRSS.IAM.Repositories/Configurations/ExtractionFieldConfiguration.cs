using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionFieldConfiguration : IEntityTypeConfiguration<ExtractionField>
	{
		public void Configure(EntityTypeBuilder<ExtractionField> builder)
		{
			builder.ToTable("extraction_fields");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).HasColumnName("field_id");

			builder.Property(x => x.TemplateId).HasColumnName("template_id").IsRequired();
			builder.Property(x => x.ParentFieldId).HasColumnName("parent_field_id");
			builder.Property(x => x.Name).HasColumnName("name").IsRequired();
			builder.Property(x => x.Instruction).HasColumnName("instruction");
			builder.Property(x => x.FieldType).HasColumnName("field_type").IsRequired();
			builder.Property(x => x.IsRequired).HasColumnName("is_required");
			builder.Property(x => x.OrderIndex).HasColumnName("order_index");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

			// Self-referencing relationship for nested structure
			builder.HasOne(x => x.ParentField)
				.WithMany(p => p.SubFields)
				.HasForeignKey(x => x.ParentFieldId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(x => x.Options)
				.WithOne(o => o.Field)
				.HasForeignKey(o => o.FieldId)
				.OnDelete(DeleteBehavior.Cascade);

			// Index for performance
			builder.HasIndex(x => x.TemplateId);
			builder.HasIndex(x => x.ParentFieldId);
		}
	}
}