using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ProjectSettingConfiguration : IEntityTypeConfiguration<ProjectSetting>
    {
        public void Configure(EntityTypeBuilder<ProjectSetting> builder)
        {
            builder.ToTable("project_settings");

            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.Id).HasColumnName("id").IsRequired();
            builder.Property(ps => ps.ProjectId).HasColumnName("project_id").IsRequired();

            builder.Property(ps => ps.ReviewersPerPaperScreening).HasColumnName("reviewers_per_paper_screening").IsRequired();
            builder.Property(ps => ps.ReviewersPerPaperQuality).HasColumnName("reviewers_per_paper_quality").IsRequired();
            builder.Property(ps => ps.ReviewersPerPaperExtraction).HasColumnName("reviewers_per_paper_extraction").IsRequired();

            builder.Property(ps => ps.AutoResolveScreeningConflicts).HasColumnName("auto_resolve_screening_conflicts").IsRequired();
            builder.Property(ps => ps.AutoResolveQualityConflicts).HasColumnName("auto_resolve_quality_conflicts").IsRequired();
            builder.Property(ps => ps.AutoResolveExtractionConflicts).HasColumnName("auto_resolve_extraction_conflicts").IsRequired();

            builder.Property(ps => ps.DeduplicationStrictness).HasColumnName("deduplication_strictness").HasConversion<string>().IsRequired();
            builder.Property(ps => ps.AutoResolveDuplicates).HasColumnName("auto_resolve_duplicates").IsRequired();

            builder.Property(ps => ps.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(ps => ps.ModifiedAt).HasColumnName("modified_at").IsRequired();

            // 1-to-1 relationship with SystematicReviewProject
            builder.HasOne(ps => ps.Project)
                .WithOne(p => p.ProjectSetting)
                .HasForeignKey<ProjectSetting>(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
