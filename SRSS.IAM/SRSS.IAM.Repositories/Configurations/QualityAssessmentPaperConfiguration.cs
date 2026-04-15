using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class QualityAssessmentPaperConfiguration : IEntityTypeConfiguration<QualityAssessmentPaper>
    {
        public void Configure(EntityTypeBuilder<QualityAssessmentPaper> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.QualityAssessmentProcessId, x.PaperId }).IsUnique();

            builder.HasOne(x => x.QualityAssessmentProcess)
                .WithMany(x => x.QualityAssessmentPapers)
                .HasForeignKey(x => x.QualityAssessmentProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Paper)
                .WithMany()
                .HasForeignKey(x => x.PaperId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}