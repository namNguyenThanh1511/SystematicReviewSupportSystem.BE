using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionSectionConfiguration : IEntityTypeConfiguration<ExtractionSection>
	{
		public void Configure(EntityTypeBuilder<ExtractionSection> builder)
		{
			builder.ToTable("extraction_section");
			builder.HasKey(x => x.Id);

			builder.Property(x => x.TemplateId).IsRequired();
			builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
			builder.Property(x => x.Description).HasMaxLength(2000);
			builder.Property(x => x.SectionType)
				.IsRequired()
				.HasDefaultValue(SectionType.FlatForm);
			builder.Property(x => x.OrderIndex).HasDefaultValue(0);

			// Relationships
			builder.HasOne(x => x.Template)
				.WithMany(x => x.Sections)
				.HasForeignKey(x => x.TemplateId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Fields)
				.WithOne(x => x.Section)
				.HasForeignKey(x => x.SectionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(x => x.TemplateId);
		}
	}
}
