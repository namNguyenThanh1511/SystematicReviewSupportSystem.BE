using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperTagConfiguration : IEntityTypeConfiguration<PaperTag>
    {
        public void Configure(EntityTypeBuilder<PaperTag> builder)
        {
            builder.ToTable("paper_tags");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(t => t.PaperId)
                .HasColumnName("paper_id")
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

            builder.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(t => t.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Prevent duplicate tags per paper/user/phase combination
            builder.HasIndex(t => new { t.PaperId, t.UserId, t.Phase, t.Label })
                .IsUnique()
                .HasDatabaseName("ix_paper_tags_paper_user_phase_label");

            builder.HasIndex(t => t.PaperId)
                .HasDatabaseName("ix_paper_tags_paper_id");

            builder.HasIndex(t => new { t.UserId, t.Phase })
                .HasDatabaseName("ix_paper_tags_user_phase");

            // Relationships
            builder.HasOne(t => t.Paper)
                .WithMany(p => p.Tags)
                .HasForeignKey(t => t.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.User)
                .WithMany(u => u.PaperTags)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
