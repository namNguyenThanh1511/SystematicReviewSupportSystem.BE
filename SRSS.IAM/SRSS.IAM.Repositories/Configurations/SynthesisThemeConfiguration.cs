using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class SynthesisThemeConfiguration : IEntityTypeConfiguration<SynthesisTheme>
    {
        public void Configure(EntityTypeBuilder<SynthesisTheme> builder)
        {
            builder.ToTable("synthesis_themes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("synthesis_theme_id")
                .IsRequired();

            builder.Property(x => x.SynthesisProcessId)
                .HasColumnName("synthesis_process_id")
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description");

            builder.Property(x => x.ColorCode)
                .HasColumnName("color_code")
                .HasMaxLength(50);

            builder.Property(x => x.CreatedById)
                .HasColumnName("created_by_id")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasOne(x => x.SynthesisProcess)
                .WithMany(x => x.Themes)
                .HasForeignKey(x => x.SynthesisProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
