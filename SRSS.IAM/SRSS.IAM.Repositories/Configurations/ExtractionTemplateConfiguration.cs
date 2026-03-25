using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionTemplateConfiguration : IEntityTypeConfiguration<ExtractionTemplate>
	{
		public void Configure(EntityTypeBuilder<ExtractionTemplate> builder)
		{
			builder.ToTable("extraction_template");
			builder.HasKey(x => x.Id);

			builder.Property(x => x.ProtocolId).IsRequired();
			builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
			builder.Property(x => x.Description).HasMaxLength(2000);

			// Relationships
			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.ExtractionTemplates)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Sections)
				.WithOne(x => x.Template)
				.HasForeignKey(x => x.TemplateId)
				.OnDelete(DeleteBehavior.Cascade);

			// Index
			builder.HasIndex(x => x.ProtocolId);
		}
	}
}