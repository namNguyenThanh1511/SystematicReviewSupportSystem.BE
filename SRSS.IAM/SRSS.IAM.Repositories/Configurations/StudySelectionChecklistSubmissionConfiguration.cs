using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionChecklistSubmissionConfiguration : IEntityTypeConfiguration<StudySelectionChecklistSubmission>
    {
        public void Configure(EntityTypeBuilder<StudySelectionChecklistSubmission> builder)
        {
            builder.ToTable("study_selection_checklist_submissions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.ScreeningDecisionId).HasColumnName("screening_decision_id").IsRequired();
            builder.Property(s => s.ChecklistTemplateId).HasColumnName("checklist_template_id").IsRequired();
            builder.Property(s => s.Version).HasColumnName("version").IsRequired();
            builder.Property(s => s.SubmittedAt).HasColumnName("submitted_at").IsRequired();
            builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(s => s.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // Relationships
            builder.HasOne(s => s.ScreeningDecision)
                .WithOne(sd => sd.ChecklistSubmission)
                .HasForeignKey<StudySelectionChecklistSubmission>(s => s.ScreeningDecisionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.ChecklistTemplate)
                .WithMany(t => t.Submissions)
                .HasForeignKey(s => s.ChecklistTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(s => s.ScreeningDecisionId).IsUnique();
        }
    }
}
