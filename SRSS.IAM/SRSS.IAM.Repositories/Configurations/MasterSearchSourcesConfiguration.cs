using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class MasterSearchSourcesConfiguration : IEntityTypeConfiguration<MasterSearchSources>
    {
        public void Configure(EntityTypeBuilder<MasterSearchSources> builder)
        {
            builder.ToTable("master_search_sources");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(m => m.SourceName)
                .HasColumnName("source_name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(m => m.BaseUrl)
                .HasColumnName("base_url")
                .HasMaxLength(1000);

            builder.Property(m => m.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(m => m.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();
        }
    }
}
