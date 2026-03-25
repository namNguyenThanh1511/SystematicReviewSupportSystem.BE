using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class UserTagInventoryConfiguration : IEntityTypeConfiguration<UserTagInventory>
    {
        public void Configure(EntityTypeBuilder<UserTagInventory> builder)
        {
            builder.ToTable("user_tag_inventory");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(t => t.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(t => t.Phase)
                .HasColumnName("phase")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(t => t.Label)
                .HasColumnName("label")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(t => t.UsageCount)
                .HasColumnName("usage_count")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(t => t.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Each user can only have one inventory entry per phase+label combination
            builder.HasIndex(t => new { t.UserId, t.Phase, t.Label })
                .IsUnique()
                .HasDatabaseName("ix_user_tag_inventory_user_phase_label");

            builder.HasIndex(t => new { t.UserId, t.Phase })
                .HasDatabaseName("ix_user_tag_inventory_user_phase");

            // Relationship
            builder.HasOne(t => t.User)
                .WithMany(u => u.TagInventory)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
