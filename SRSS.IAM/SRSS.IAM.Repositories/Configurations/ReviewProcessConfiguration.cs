using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ReviewProcessConfiguration : IEntityTypeConfiguration<ReviewProcess>
    {
        public void Configure(EntityTypeBuilder<ReviewProcess> builder)
        {
            builder.ToTable("review_processes");

            builder.HasKey(rp => rp.Id);

            builder.Property(rp => rp.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(rp => rp.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();


            builder.Property(rp => rp.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(rp => rp.CurrentPhase)
                .HasColumnName("current_phase")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(rp => rp.StartedAt)
                .HasColumnName("started_at");

            builder.Property(rp => rp.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(rp => rp.Notes)
                .HasColumnName("notes")
                .HasMaxLength(2000);

            builder.Property(rp => rp.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(rp => rp.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasOne(rp => rp.Project)
                .WithMany(p => p.ReviewProcesses)
                .HasForeignKey(rp => rp.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(rp => rp.IdentificationProcesses)
                .WithOne(ip => ip.ReviewProcess)
                .HasForeignKey(ip => ip.ReviewProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(rp => rp.ProjectId)
                .HasDatabaseName("idx_review_process_project_id");

            builder.HasIndex(rp => rp.Status)
                .HasDatabaseName("idx_review_process_status");

        }
    }
}
