using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.ToTable("project_members");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasColumnName("id");

            builder.Property(x => x.ProjectId)
                   .HasColumnName("project_id")
                   .IsRequired();

            builder.Property(x => x.UserId)
                   .HasColumnName("user_id")
                   .IsRequired();

            builder.Property(x => x.Role)
                   .HasColumnName("role")
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.JoinedAt)
                   .HasColumnName("joined_at")
                   .HasDefaultValueSql("timezone('utc', now())")
                   .IsRequired();

            // Relationships
            builder.HasOne(x => x.Project)
                   .WithMany(p => p.ProjectMembers)
                   .HasForeignKey(x => x.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate user in project
            builder.HasIndex(x => new { x.ProjectId, x.UserId })
                   .IsUnique();

            // Ensure only ONE leader per project
            builder.HasIndex(x => new { x.ProjectId, x.Role })
                   .IsUnique()
                   .HasFilter("role = 1");

            // Query performance indexes
            builder.HasIndex(x => x.ProjectId);
            builder.HasIndex(x => x.UserId);
        }
    }
}