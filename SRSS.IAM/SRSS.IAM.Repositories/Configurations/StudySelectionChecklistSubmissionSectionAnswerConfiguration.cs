using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionChecklistSubmissionSectionAnswerConfiguration : IEntityTypeConfiguration<StudySelectionChecklistSubmissionSectionAnswer>
    {
        public void Configure(EntityTypeBuilder<StudySelectionChecklistSubmissionSectionAnswer> builder)
        {
            builder.ToTable("study_selection_checklist_submission_section_answers");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id).HasColumnName("id");
            builder.Property(a => a.SubmissionId).HasColumnName("submission_id").IsRequired();
            builder.Property(a => a.SectionId).HasColumnName("section_id").IsRequired();
            builder.Property(a => a.IsChecked).HasColumnName("is_checked").IsRequired();

            builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(a => a.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // Relationships
            builder.HasOne(a => a.Submission)
                .WithMany(s => s.SectionAnswers)
                .HasForeignKey(a => a.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Section)
                .WithMany(s => s.SectionAnswers)
                .HasForeignKey(a => a.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(a => new { a.SubmissionId, a.SectionId }).IsUnique();
        }
    }
}
