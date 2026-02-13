using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PrismaReportConfiguration : IEntityTypeConfiguration<PrismaReport>
    {
        public void Configure(EntityTypeBuilder<PrismaReport> builder)
        {
            builder.ToTable("prisma_reports");

            builder.HasKey(pr => pr.Id);

            builder.Property(pr => pr.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(pr => pr.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(pr => pr.Version)
                .HasColumnName("version")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(pr => pr.GeneratedAt)
                .HasColumnName("generated_at")
                .IsRequired();

            builder.Property(pr => pr.Notes)
                .HasColumnName("notes");

            builder.Property(pr => pr.GeneratedBy)
                .HasColumnName("generated_by")
                .HasMaxLength(255);

            builder.Property(pr => pr.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(pr => pr.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationship with Project
            builder.HasOne(pr => pr.Project)
                .WithMany()
                .HasForeignKey(pr => pr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with FlowRecords
            builder.HasMany(pr => pr.FlowRecords)
                .WithOne(fr => fr.PrismaReport)
                .HasForeignKey(fr => fr.PrismaReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pr => pr.ProjectId);
            builder.HasIndex(pr => pr.GeneratedAt);
        }
    }
}
