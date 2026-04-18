using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ScreeningResolutionConfiguration : IEntityTypeConfiguration<ScreeningResolution>
    {
        public void Configure(EntityTypeBuilder<ScreeningResolution> builder)
        {
            builder.ToTable("screening_resolutions");

            builder.HasKey(sr => sr.Id);

            builder.Property(sr => sr.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(sr => sr.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(sr => sr.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(sr => sr.FinalDecision)
                .HasColumnName("final_decision")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(sr => sr.ExclusionReasonId)
                .HasColumnName("exclusion_reason_id");

            builder.Property(sr => sr.ResolutionNotes)
                .HasColumnName("resolution_notes");

            builder.Property(sr => sr.Phase)
                .HasColumnName("screening_phase")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(sr => sr.ResolvedBy)
                .HasColumnName("resolved_by")
                .IsRequired();

            builder.Property(sr => sr.ResolvedAt)
                .HasColumnName("resolved_at")
                .IsRequired();

            builder.Property(sr => sr.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(sr => sr.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(sr => sr.StudySelectionProcess)
                .WithMany(ssp => ssp.ScreeningResolutions)
                .HasForeignKey(sr => sr.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sr => sr.Paper)
                .WithMany(p => p.ScreeningResolutions)
                .HasForeignKey(sr => sr.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sr => sr.ExclusionReason)
                .WithMany(er => er.ScreeningResolutions)
                .HasForeignKey(sr => sr.ExclusionReasonId)
                .OnDelete(DeleteBehavior.SetNull);

            // Unique constraint: One resolution per paper per selection process
            builder.HasIndex(sr => new
            {
                sr.StudySelectionProcessId,
                sr.PaperId,
                sr.Phase
            })
            .IsUnique()
            .HasDatabaseName("uq_screening_resolution_process_paper_phase");

            // Additional indexes
            builder.HasIndex(sr => sr.PaperId);
            builder.HasIndex(sr => sr.ResolvedBy);
        }
    }
}
