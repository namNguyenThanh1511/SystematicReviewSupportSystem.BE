using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Configurations
{
    public class FullTextScreeningConfiguration : IEntityTypeConfiguration<FullTextScreening>
    {
        public void Configure(EntityTypeBuilder<FullTextScreening> builder)
        {
            builder.ToTable("full_text_screenings");

            builder.HasKey(ft => ft.Id);

            builder.Property(ft => ft.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ft => ft.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(ft => ft.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ft => ft.StartedAt)
                .HasColumnName("started_at");

            builder.Property(ft => ft.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(ft => ft.MinReviewersPerPaper)
                .HasColumnName("min_reviewers_per_paper")
                .HasDefaultValue(2)
                .IsRequired();

            builder.Property(ft => ft.MaxReviewersPerPaper)
                .HasColumnName("max_reviewers_per_paper")
                .HasDefaultValue(3)
                .IsRequired();

            builder.Property(ft => ft.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ft => ft.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // 1:1 relationship with StudySelectionProcess
            builder.HasOne(ft => ft.StudySelectionProcess)
                .WithOne(ssp => ssp.FullTextScreening)
                .HasForeignKey<FullTextScreening>(ft => ft.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enforce 1:1 at database level
            builder.HasIndex(ft => ft.StudySelectionProcessId)
                .IsUnique()
                .HasDatabaseName("idx_ft_screening_study_selection_process_id_unique");
        }
    }
}
