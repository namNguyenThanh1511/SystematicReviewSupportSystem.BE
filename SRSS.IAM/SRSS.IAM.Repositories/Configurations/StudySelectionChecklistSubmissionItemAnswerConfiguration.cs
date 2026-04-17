using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionChecklistSubmissionItemAnswerConfiguration : IEntityTypeConfiguration<StudySelectionChecklistSubmissionItemAnswer>
    {
        public void Configure(EntityTypeBuilder<StudySelectionChecklistSubmissionItemAnswer> builder)
        {
            builder.ToTable("study_selection_checklist_submission_item_answers");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id).HasColumnName("id");
            builder.Property(a => a.SubmissionId).HasColumnName("submission_id").IsRequired();
            builder.Property(a => a.ItemId).HasColumnName("item_id").IsRequired();
            builder.Property(a => a.IsChecked).HasColumnName("is_checked").IsRequired();

            builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(a => a.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // Relationships
            builder.HasOne(a => a.Submission)
                .WithMany(s => s.ItemAnswers)
                .HasForeignKey(a => a.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Item)
                .WithMany(i => i.ItemAnswers)
                .HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(a => new { a.SubmissionId, a.ItemId }).IsUnique();
        }
    }
}
