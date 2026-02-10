using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(u => u.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.Password)
                .HasColumnName("password")
                .HasMaxLength(255)
                .IsRequired(false);

            builder.Property(u => u.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(u => u.RefreshToken)
                .HasColumnName("refresh_token")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(u => u.IsRefreshTokenRevoked)
                .HasColumnName("is_refresh_token_revoked")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(u => u.RefreshTokenExpiryTime)
                .HasColumnName("refresh_token_expiry_time")
                .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(u => u.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Indexes
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("ix_users_email");

            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("ix_users_username");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("ix_users_is_active");
        }
    }
}
