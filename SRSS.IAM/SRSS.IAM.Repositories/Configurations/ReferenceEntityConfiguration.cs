using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ReferenceEntityConfiguration : IEntityTypeConfiguration<ReferenceEntity>
    {
        public void Configure(EntityTypeBuilder<ReferenceEntity> builder)
        {
            builder.ToTable("reference_entities");

            builder.HasKey(re => re.Id);

            builder.Property(re => re.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(re => re.Title)
                .HasColumnName("title")
                .HasMaxLength(1000);

            builder.Property(re => re.Authors)
                .HasColumnName("authors")
                .HasMaxLength(2000);

            builder.Property(re => re.DOI)
                .HasColumnName("doi")
                .HasMaxLength(255);

            builder.Property(re => re.Url)
                .HasColumnName("url")
                .HasMaxLength(2000);

            builder.Property(re => re.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(re => re.RawReference)
                .HasColumnName("raw_reference");

            builder.Property(re => re.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(re => re.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Indexes
            builder.HasIndex(re => re.DOI);
            builder.HasIndex(re => re.Type);
        }
    }
}
