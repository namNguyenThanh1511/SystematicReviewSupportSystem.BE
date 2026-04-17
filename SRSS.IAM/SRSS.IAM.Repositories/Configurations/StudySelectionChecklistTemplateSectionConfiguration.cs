using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionChecklistTemplateSectionConfiguration : IEntityTypeConfiguration<StudySelectionChecklistTemplateSection>
    {
        public void Configure(EntityTypeBuilder<StudySelectionChecklistTemplateSection> builder)
        {
            builder.ToTable("study_selection_checklist_template_sections");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.TemplateId).HasColumnName("template_id").IsRequired();
            builder.Property(s => s.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            builder.Property(s => s.Description).HasColumnName("description");
            builder.Property(s => s.Order).HasColumnName("display_order").IsRequired();
            builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(s => s.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // Relationships
            builder.HasOne(s => s.Template)
                .WithMany(t => t.Sections)
                .HasForeignKey(s => s.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(s => s.TemplateId);
        }
    }
}
