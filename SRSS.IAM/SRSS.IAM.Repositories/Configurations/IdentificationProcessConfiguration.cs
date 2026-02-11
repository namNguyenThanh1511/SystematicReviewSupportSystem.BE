using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class IdentificationProcessConfiguration : IEntityTypeConfiguration<IdentificationProcess>
    {
        public void Configure(EntityTypeBuilder<IdentificationProcess> builder)
        {
            builder.ToTable("identification_processes");

            builder.HasKey(ip => ip.Id);

            builder.Property(ip => ip.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ip => ip.ReviewProcessId)
                .HasColumnName("review_process_id")
                .IsRequired();

            builder.Property(ip => ip.Notes)
                .HasColumnName("notes");

            builder.Property(ip => ip.StartedAt)
                .HasColumnName("started_at");

            builder.Property(ip => ip.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(ip => ip.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ip => ip.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ip => ip.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasMany(ip => ip.SearchExecutions)
                .WithOne(se => se.IdentificationProcess)
                .HasForeignKey(se => se.IdentificationProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ip => ip.ReviewProcess)
                .WithMany(rp => rp.IdentificationProcesses)
                .HasForeignKey(ip => ip.ReviewProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
