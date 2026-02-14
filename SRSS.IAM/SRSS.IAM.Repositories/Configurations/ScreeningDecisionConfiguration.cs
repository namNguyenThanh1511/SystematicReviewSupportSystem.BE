using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ScreeningDecisionConfiguration : IEntityTypeConfiguration<ScreeningDecision>
    {
        public void Configure(EntityTypeBuilder<ScreeningDecision> builder)
        {
            builder.ToTable("screening_decisions");

            builder.HasKey(sd => sd.Id);

            builder.Property(sd => sd.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(sd => sd.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(sd => sd.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(sd => sd.ReviewerId)
                .HasColumnName("reviewer_id")
                .IsRequired();

            builder.Property(sd => sd.Decision)
                .HasColumnName("decision")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(sd => sd.Reason)
                .HasColumnName("reason");

            builder.Property(sd => sd.DecidedAt)
                .HasColumnName("decided_at")
                .IsRequired();

            builder.Property(sd => sd.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(sd => sd.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(sd => sd.StudySelectionProcess)
                .WithMany(ssp => ssp.ScreeningDecisions)
                .HasForeignKey(sd => sd.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sd => sd.Paper)
                .WithMany(p => p.ScreeningDecisions)
                .HasForeignKey(sd => sd.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(sd => sd.StudySelectionProcessId);
            builder.HasIndex(sd => sd.PaperId);
            builder.HasIndex(sd => sd.ReviewerId);
            builder.HasIndex(sd => new { sd.StudySelectionProcessId, sd.PaperId, sd.ReviewerId });
        }
    }
}
