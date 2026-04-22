using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ProjectPicocConfiguration : IEntityTypeConfiguration<ProjectPicoc>
    {
        public void Configure(EntityTypeBuilder<ProjectPicoc> builder)
        {
            builder.ToTable("project_picocs");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(p => p.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(p => p.Population)
                .HasColumnName("population")
                .HasMaxLength(2000);

            builder.Property(p => p.Intervention)
                .HasColumnName("intervention")
                .HasMaxLength(2000);

            builder.Property(p => p.Comparator)
                .HasColumnName("comparator")
                .HasMaxLength(2000);

            builder.Property(p => p.Outcome)
                .HasColumnName("outcome")
                .HasMaxLength(2000);

            builder.Property(p => p.Context)
                .HasColumnName("context")
                .HasMaxLength(2000);

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(p => p.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationship
            builder.HasOne(p => p.Project)
                .WithMany(pj => pj.ProjectPicocs)
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
