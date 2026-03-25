using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class StudySelectionProcessPaperConfiguration : IEntityTypeConfiguration<StudySelectionProcessPaper>
    {
        public void Configure(EntityTypeBuilder<StudySelectionProcessPaper> builder)
        {
            builder.ToTable("study_selection_process_papers");

            builder.HasKey(spp => spp.Id);

            builder.Property(spp => spp.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(spp => spp.StudySelectionProcessId)
                .HasColumnName("study_selection_process_id")
                .IsRequired();

            builder.Property(spp => spp.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(spp => spp.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(spp => spp.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(spp => spp.StudySelectionProcess)
                .WithMany(ssp => ssp.StudySelectionProcessPapers)
                .HasForeignKey(spp => spp.StudySelectionProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(spp => spp.Paper)
                .WithMany(p => p.StudySelectionProcessPapers)
                .HasForeignKey(spp => spp.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: One entry per paper per selection process
            builder.HasIndex(spp => new { spp.StudySelectionProcessId, spp.PaperId })
                .IsUnique()
                .HasDatabaseName("uq_study_selection_process_paper");

            // Additional indexes for performance
            builder.HasIndex(spp => spp.StudySelectionProcessId);
            builder.HasIndex(spp => spp.PaperId);
        }
    }
}
