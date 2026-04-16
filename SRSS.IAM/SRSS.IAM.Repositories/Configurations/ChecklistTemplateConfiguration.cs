using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ChecklistTemplateConfiguration : IEntityTypeConfiguration<ChecklistTemplate>
    {
        public void Configure(EntityTypeBuilder<ChecklistTemplate> builder)
        {
            builder.ToTable("checklist_templates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            builder.Property(x => x.IsSystem)
                .HasColumnName("is_system")
                .IsRequired();

            builder.Property(x => x.Version)
                .HasColumnName("version")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Ignore(x => x.ModifiedAt);
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasMany(x => x.ItemTemplates)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ReviewChecklists)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.IsSystem, x.Name })
                .HasDatabaseName("idx_checklist_templates_system_name");
        }
    }
}
