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
            builder.Property(s => s.StudySelectionProcessId).HasColumnName("study_selection_process_id").IsRequired();
            builder.Property(s => s.PaperId).HasColumnName("paper_id").IsRequired();
            builder.Property(s => s.ReviewerId).HasColumnName("reviewer_id").IsRequired();
            builder.Property(s => s.Phase)
                .HasColumnName("screening_phase")
                .HasConversion<string>()
                .IsRequired();
            builder.Property(s => s.ChecklistTemplateId).HasColumnName("checklist_template_id").IsRequired();
            builder.Property(s => s.SubmittedAt).HasColumnName("submitted_at");
            builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(s => s.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // Relationships
            builder.HasOne(s => s.ChecklistTemplate)
                .WithMany(t => t.Submissions)
                .HasForeignKey(s => s.ChecklistTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(s => s.StudySelectionProcessId);
            builder.HasIndex(s => s.PaperId);
            builder.HasIndex(s => s.ReviewerId);

            // Unique constraint: one submission per reviewer per paper per phase
            builder.HasIndex(s => new
            {
                s.StudySelectionProcessId,
                s.PaperId,
                s.ReviewerId,
                s.Phase
            })
            .IsUnique()
            .HasDatabaseName("uq_study_selection_checklist_submission_context");
        }
    }
}
