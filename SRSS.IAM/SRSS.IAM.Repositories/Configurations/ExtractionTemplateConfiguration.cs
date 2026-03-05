using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionTemplateConfiguration : IEntityTypeConfiguration<ExtractionTemplate>
	{
		public void Configure(EntityTypeBuilder<ExtractionTemplate> builder)
		{
			builder.ToTable("extraction_templates");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).HasColumnName("template_id");

			builder.Property(x => x.ProtocolId).HasColumnName("protocol_id").IsRequired();
			builder.Property(x => x.Name).HasColumnName("name").IsRequired();
			builder.Property(x => x.Description).HasColumnName("description");
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

			// Relationships
			builder.HasOne(x => x.Protocol)
				.WithMany(p => p.ExtractionTemplates)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Fields)
				.WithOne(f => f.Template)
				.HasForeignKey(f => f.TemplateId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}