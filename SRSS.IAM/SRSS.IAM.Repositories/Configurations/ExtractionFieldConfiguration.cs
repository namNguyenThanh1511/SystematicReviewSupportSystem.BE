using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionFieldConfiguration : IEntityTypeConfiguration<ExtractionField>
	{
		public void Configure(EntityTypeBuilder<ExtractionField> builder)
		{
			builder.ToTable("extraction_field");
			builder.HasKey(x => x.Id);

			builder.Property(x => x.SectionId).IsRequired();
			builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
			builder.Property(x => x.Instruction).HasMaxLength(2000);
			builder.Property(x => x.FieldType)
				.IsRequired()
				.HasDefaultValue(FieldType.Text);
			builder.Property(x => x.OrderIndex).HasDefaultValue(0);

			// Relationships
			builder.HasOne(x => x.Section)
				.WithMany(x => x.Fields)
				.HasForeignKey(x => x.SectionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Self-referencing
			builder.HasOne(x => x.ParentField)
				.WithMany(x => x.SubFields)
				.HasForeignKey(x => x.ParentFieldId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Options)
				.WithOne(x => x.Field)
				.HasForeignKey(x => x.FieldId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.SubFields)
				.WithOne(x => x.ParentField)
				.HasForeignKey(x => x.ParentFieldId)
				.OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(x => x.SectionId);
			builder.HasIndex(x => x.ParentFieldId);
		}
	}
}