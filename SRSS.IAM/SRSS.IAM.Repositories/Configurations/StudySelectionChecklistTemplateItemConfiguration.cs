using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionChecklistTemplateItemConfiguration : IEntityTypeConfiguration<StudySelectionChecklistTemplateItem>
    {
        public void Configure(EntityTypeBuilder<StudySelectionChecklistTemplateItem> builder)
        {
            builder.ToTable("study_selection_checklist_template_items");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id).HasColumnName("id");
            builder.Property(i => i.SectionId).HasColumnName("section_id").IsRequired();
            builder.Property(i => i.Text).HasColumnName("text").IsRequired();
            builder.Property(i => i.Order).HasColumnName("display_order").IsRequired();
            builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(i => i.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // Relationships
            builder.HasOne(i => i.Section)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(i => i.SectionId);
        }
    }
}
