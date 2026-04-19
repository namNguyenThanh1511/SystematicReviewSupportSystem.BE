using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionAIResultConfiguration : IEntityTypeConfiguration<StudySelectionAIResult>
    {
        public void Configure(EntityTypeBuilder<StudySelectionAIResult> builder)
        {
            builder.ToTable("study_selection_ai_results");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(x => x.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(x => x.ReviewerId)
                .HasColumnName("reviewer_id")
                .IsRequired();

            builder.Property(x => x.Phase)
                .HasColumnName("screening_phase")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.AIOutputJson)
                .HasColumnName("ai_output_json")
                .IsRequired();

            builder.Property(x => x.RelevanceScore)
                .HasColumnName("relevance_score")
                .IsRequired();

            builder.Property(x => x.Recommendation)
                .HasColumnName("recommendation")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("updated_at") // Using updated_at per SQL script and user request
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.StudySelectionProcess)
                .WithMany(p => p.StudySelectionAIResults)
                .HasForeignKey(x => x.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Paper)
                .WithMany(p => p.StudySelectionAIResults)
                .HasForeignKey(x => x.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Reviewer)
                .WithMany(u => u.StudySelectionAIResults)
                .HasForeignKey(x => x.ReviewerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint
            builder.HasIndex(x => new
            {
                x.StudySelectionProcessId,
                x.PaperId,
                x.ReviewerId,
                x.Phase
            })
            .IsUnique()
            .HasDatabaseName("uq_ss_ai_results_process_paper_reviewer_phase");
        }
    }
}
