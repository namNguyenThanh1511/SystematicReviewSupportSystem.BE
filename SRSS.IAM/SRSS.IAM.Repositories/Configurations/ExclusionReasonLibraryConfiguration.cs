using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ExclusionReasonLibraryConfiguration : IEntityTypeConfiguration<ExclusionReasonLibrary>
    {
        public void Configure(EntityTypeBuilder<ExclusionReasonLibrary> builder)
        {
            builder.ToTable("exclusion_reason_libraries");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(e => e.Code)
                .HasColumnName("code")
                .IsRequired();

            builder.HasIndex(e => e.Code)
                .IsUnique()
                .HasDatabaseName("ix_exclusion_reason_libraries_code");

            builder.Property(e => e.Name)
                .HasColumnName("name")
                .HasColumnType("citext")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Unique name (Case-Insensitive)
            builder.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("ix_exclusion_reason_libraries_name");
        }
    }
}
