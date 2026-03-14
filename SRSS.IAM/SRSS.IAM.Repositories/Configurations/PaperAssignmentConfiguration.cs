using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperAssignmentConfiguration : IEntityTypeConfiguration<PaperAssignment>
    {
        public void Configure(EntityTypeBuilder<PaperAssignment> builder)
        {
            builder.ToTable("paper_assignments");

            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(pa => pa.PaperId)
                .HasColumnName("paper_id")
                .IsRequired();

            builder.Property(pa => pa.ProjectMemberId)
                .HasColumnName("project_member_id")
                .IsRequired();

            builder.Property(pa => pa.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(pa => pa.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(pa => pa.Paper)
                .WithMany(p => p.PaperAssignments)
                .HasForeignKey(pa => pa.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pa => pa.ProjectMember)
                .WithMany(pm => pm.PaperAssignments)
                .HasForeignKey(pa => pa.ProjectMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: A paper can be assigned to the same reviewer only once
            builder.HasIndex(pa => new { pa.PaperId, pa.ProjectMemberId })
                .IsUnique()
                .HasDatabaseName("uq_paper_assignment_paper_member");

            // Additional indexes for performance
            builder.HasIndex(pa => pa.PaperId);
            builder.HasIndex(pa => pa.ProjectMemberId);
        }
    }
}
