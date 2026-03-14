using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Configurations
{
    public class TitleAbstractScreeningConfiguration : IEntityTypeConfiguration<TitleAbstractScreening>
    {
        public void Configure(EntityTypeBuilder<TitleAbstractScreening> builder)
        {
            builder.ToTable("title_abstract_screenings");

            builder.HasKey(ta => ta.Id);

            builder.Property(ta => ta.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ta => ta.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(ta => ta.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ta => ta.StartedAt)
                .HasColumnName("started_at");

            builder.Property(ta => ta.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(ta => ta.MinReviewersPerPaper)
                .HasColumnName("min_reviewers_per_paper")
                .HasDefaultValue(2)
                .IsRequired();

            builder.Property(ta => ta.MaxReviewersPerPaper)
                .HasColumnName("max_reviewers_per_paper")
                .HasDefaultValue(3)
                .IsRequired();

            builder.Property(ta => ta.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ta => ta.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // 1:1 relationship with StudySelectionProcess
            builder.HasOne(ta => ta.StudySelectionProcess)
                .WithOne(ssp => ssp.TitleAbstractScreening)
                .HasForeignKey<TitleAbstractScreening>(ta => ta.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enforce 1:1 at database level
            builder.HasIndex(ta => ta.StudySelectionProcessId)
                .IsUnique()
                .HasDatabaseName("idx_ta_screening_study_selection_process_id_unique");
        }
    }
}
