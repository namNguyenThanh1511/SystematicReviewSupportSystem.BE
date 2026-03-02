using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(n => n.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(n => n.Title)
                .HasColumnName("title")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(n => n.Message)
                .HasColumnName("message")
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(n => n.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(n => n.NavigationUrl)
                .HasColumnName("navigation_url")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(n => n.IsRead)
                .HasColumnName("is_read")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(n => n.ReadAt)
                .HasColumnName("read_at")
                .IsRequired(false);

            builder.Property(n => n.Metadata)
                .HasColumnName("metadata")
                .IsRequired(false);

            builder.Property(n => n.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(n => n.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Relationships
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(n => n.UserId)
                .HasDatabaseName("ix_notifications_user_id");

            builder.HasIndex(n => n.IsRead)
                .HasDatabaseName("ix_notifications_is_read");
        }
    }
}
