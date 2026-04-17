using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionChecklistTemplateConfiguration : IEntityTypeConfiguration<StudySelectionChecklistTemplate>
    {
        public void Configure(EntityTypeBuilder<StudySelectionChecklistTemplate> builder)
        {
            builder.ToTable("study_selection_checklist_templates");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id).HasColumnName("id");
            builder.Property(t => t.ProjectId).HasColumnName("project_id").IsRequired();
            builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            builder.Property(t => t.Description).HasColumnName("description");
            builder.Property(t => t.Version).HasColumnName("version").IsRequired().HasDefaultValue(1);
            builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(t => t.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // Relationships
            builder.HasOne(t => t.Project)
                .WithMany(p => p.StudySelectionChecklistTemplates)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.ProjectId);
        }
    }
}
