using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionExclusionReasonConfiguration : IEntityTypeConfiguration<StudySelectionExclusionReason>
    {
        public void Configure(EntityTypeBuilder<StudySelectionExclusionReason> builder)
        {
            builder.ToTable("study_selection_exclusion_reasons");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(e => e.LibraryReasonId)
                .HasColumnName("library_reason_id");

            builder.Property(e => e.Code)
                .HasColumnName("code")
                .IsRequired();

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("citext")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.StudySelectionProcess)
                .WithMany(ssp => ssp.ExclusionReasons)
                .HasForeignKey(e => e.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.LibraryReason)
                .WithMany()
                .HasForeignKey(e => e.LibraryReasonId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1. Index for fast process lookup
            builder.HasIndex(e => e.StudySelectionProcessId)
                .HasDatabaseName("idx_study_selection_exclusion_reasons_process_id");

            // 2. Unique constraint: Prevent duplicate reason codes inside a process
            builder.HasIndex(e => new { e.StudySelectionProcessId, e.Code })
                .IsUnique()
                .HasDatabaseName("ux_study_selection_exclusion_reasons_process_code");

            // 3. Unique constraint: Prevent adding the same library reason twice in one process
            builder.HasIndex(e => new { e.StudySelectionProcessId, e.LibraryReasonId })
                .IsUnique()
                .HasFilter("library_reason_id IS NOT NULL")
                .HasDatabaseName("ux_study_selection_exclusion_reasons_process_library");

            // 4. Performance index for Code
            builder.HasIndex(e => e.Code)
                .HasDatabaseName("idx_study_selection_exclusion_reasons_code");

            // 5. Unique constraint: Prevent duplicate names inside a process (Case-Insensitive)
            builder.HasIndex(e => new { e.StudySelectionProcessId, e.Name })
                .IsUnique()
                .HasDatabaseName("ux_study_selection_exclusion_reasons_process_name");
        }
    }
}
