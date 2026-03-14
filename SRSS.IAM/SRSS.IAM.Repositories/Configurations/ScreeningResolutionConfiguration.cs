using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

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

            builder.Property(sr => sr.ResolutionNotes)
                .HasColumnName("resolution_notes");

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

            // Unique constraint: One resolution per paper per selection process
            builder.HasIndex(sr => new { sr.StudySelectionProcessId, sr.PaperId })
                .IsUnique();

            // Additional indexes
            builder.HasIndex(sr => sr.PaperId);
            builder.HasIndex(sr => sr.ResolvedBy);
        }
    }
}
