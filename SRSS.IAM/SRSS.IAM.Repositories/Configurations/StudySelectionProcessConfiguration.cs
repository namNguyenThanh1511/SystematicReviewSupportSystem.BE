using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionProcessConfiguration : IEntityTypeConfiguration<StudySelectionProcess>
    {
        public void Configure(EntityTypeBuilder<StudySelectionProcess> builder)
        {
            builder.ToTable("study_selection_processes");

            builder.HasKey(ssp => ssp.Id);

            builder.Property(ssp => ssp.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ssp => ssp.ReviewProcessId)
                .HasColumnName("review_process_id")
                .IsRequired();

            builder.Property(ssp => ssp.Notes)
                .HasColumnName("notes");

            builder.Property(ssp => ssp.StartedAt)
                .HasColumnName("started_at");

            builder.Property(ssp => ssp.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(ssp => ssp.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ssp => ssp.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ssp => ssp.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(ssp => ssp.ReviewProcess)
                .WithOne(rp => rp.StudySelectionProcess)
                .HasForeignKey<StudySelectionProcess>(ssp => ssp.ReviewProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ssp => ssp.ScreeningDecisions)
                .WithOne(sd => sd.StudySelectionProcess)
                .HasForeignKey(sd => sd.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ssp => ssp.ScreeningResolutions)
                .WithOne(sr => sr.StudySelectionProcess)
                .HasForeignKey(sr => sr.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enforce 1:1 relationship at database level
            builder.HasIndex(ssp => ssp.ReviewProcessId)
                .IsUnique()
                .HasDatabaseName("idx_study_selection_process_review_process_id_unique");
        }
    }
}
