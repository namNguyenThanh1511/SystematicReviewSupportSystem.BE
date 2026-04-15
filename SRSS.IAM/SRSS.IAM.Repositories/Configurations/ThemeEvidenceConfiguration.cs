using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ThemeEvidenceConfiguration : IEntityTypeConfiguration<ThemeEvidence>
    {
        public void Configure(EntityTypeBuilder<ThemeEvidence> builder)
        {
            builder.ToTable("theme_evidences");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("theme_evidence_id")
                .IsRequired();

            builder.Property(x => x.ThemeId)
                .HasColumnName("theme_id")
                .IsRequired();

            builder.Property(x => x.ExtractedDataValueId)
                .HasColumnName("extracted_data_value_id")
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasColumnName("notes");

            builder.Property(x => x.CreatedById)
                .HasColumnName("created_by_id")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasOne(x => x.Theme)
                .WithMany(x => x.Evidences)
                .HasForeignKey(x => x.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtractedDataValue)
                .WithMany()
                .HasForeignKey(x => x.ExtractedDataValueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
