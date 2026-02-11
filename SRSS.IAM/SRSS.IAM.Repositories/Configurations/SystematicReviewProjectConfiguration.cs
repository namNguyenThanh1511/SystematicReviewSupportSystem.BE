using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class SystematicReviewProjectConfiguration : IEntityTypeConfiguration<SystematicReviewProject>
    {
        public void Configure(EntityTypeBuilder<SystematicReviewProject> builder)
        {
            builder.ToTable("systematic_review_projects");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(p => p.Title)
                .HasColumnName("title")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(p => p.Domain)
                .HasColumnName("domain")
                .HasMaxLength(255);

            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            builder.Property(p => p.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(p => p.StartDate)
                .HasColumnName("start_date");

            builder.Property(p => p.EndDate)
                .HasColumnName("end_date");

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(p => p.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // One-to-Many Relationship: SystematicReviewProject ? ReviewProcesses
            builder.HasMany(p => p.ReviewProcesses)
                .WithOne(rp => rp.Project)
                .HasForeignKey(rp => rp.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.Status)
                .HasDatabaseName("idx_project_status");

            builder.HasIndex(p => p.Title)
                .HasDatabaseName("idx_project_title");
        }
    }
}
