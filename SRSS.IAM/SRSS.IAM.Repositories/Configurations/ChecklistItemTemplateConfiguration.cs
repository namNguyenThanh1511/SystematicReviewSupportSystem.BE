using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ChecklistItemTemplateConfiguration : IEntityTypeConfiguration<ChecklistItemTemplate>
    {
        public void Configure(EntityTypeBuilder<ChecklistItemTemplate> builder)
        {
            builder.ToTable("checklist_item_templates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.TemplateId)
                .HasColumnName("template_id")
                .IsRequired();

            builder.Property(x => x.SectionId)
                .HasColumnName("section_id");

            builder.Property(x => x.ParentId)
                .HasColumnName("parent_id");

            builder.Property(x => x.ItemNumber)
                .HasColumnName("item_number")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Section)
                .HasColumnName("section")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Topic)
                .HasColumnName("topic")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(x => x.Order)
                .HasColumnName("order_index")
                .IsRequired();

            builder.Property(x => x.IsRequired)
                .HasColumnName("is_required")
                .IsRequired();

            builder.Property(x => x.HasLocationField)
                .HasColumnName("has_location_field")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.IsSectionHeaderOnly)
                .HasColumnName("is_section_header_only")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.DefaultSampleAnswer)
                .HasColumnName("default_sample_answer")
                .HasMaxLength(4000);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SectionTemplate)
                .WithMany(x => x.ItemTemplates)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Responses)
                .WithOne(x => x.ItemTemplate)
                .HasForeignKey(x => x.ItemTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TemplateId, x.Order })
                .HasDatabaseName("idx_checklist_item_templates_template_order");

            builder.HasIndex(x => x.ParentId)
                .HasDatabaseName("idx_checklist_item_templates_parent_id");

            builder.HasIndex(x => x.SectionId)
                .HasDatabaseName("idx_checklist_item_templates_section_id");

            builder.HasIndex(x => new { x.TemplateId, x.ItemNumber })
                .IsUnique()
                .HasDatabaseName("ux_checklist_item_templates_template_item_number");
        }
    }
}
