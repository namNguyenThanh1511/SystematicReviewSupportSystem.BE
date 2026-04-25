using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionCriteriaAIResponseConfiguration : IEntityTypeConfiguration<StudySelectionCriteriaAIResponse>
    {
        public void Configure(EntityTypeBuilder<StudySelectionCriteriaAIResponse> builder)
        {
            builder.ToTable("study_selection_criteria_ai_responses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(x => x.RawJson)
                .HasColumnName("raw_json")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.StudySelectionProcess)
                .WithMany(ssp => ssp.StudySelectionCriteriaAIResponses)
                .HasForeignKey(x => x.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
