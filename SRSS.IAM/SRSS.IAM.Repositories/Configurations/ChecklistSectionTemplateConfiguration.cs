using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ChecklistSectionTemplateConfiguration : IEntityTypeConfiguration<ChecklistSectionTemplate>
    {
        public void Configure(EntityTypeBuilder<ChecklistSectionTemplate> builder)
        {
            builder.ToTable("checklist_section_templates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.TemplateId)
                .HasColumnName("template_id")
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            builder.Property(x => x.Order)
                .HasColumnName("order_index")
                .IsRequired();

            builder.Property(x => x.SectionNumber)
                .HasColumnName("section_number")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasIndex(x => new { x.TemplateId, x.Order })
                .HasDatabaseName("idx_checklist_section_templates_template_order");

            builder.HasIndex(x => new { x.TemplateId, x.SectionNumber })
                .IsUnique()
                .HasDatabaseName("ux_checklist_section_templates_template_section_number");
        }
    }
}