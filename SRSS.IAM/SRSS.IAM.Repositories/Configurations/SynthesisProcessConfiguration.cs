using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class SynthesisProcessConfiguration : IEntityTypeConfiguration<SynthesisProcess>
    {
        public void Configure(EntityTypeBuilder<SynthesisProcess> builder)
        {
            builder.ToTable("synthesis_processes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("synthesis_process_id")
                .IsRequired();

            builder.Property(x => x.ReviewProcessId)
                .HasColumnName("review_process_id")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.StartedAt)
                .HasColumnName("started_at");

            builder.Property(x => x.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasOne(x => x.ReviewProcess)
                .WithOne(rp => rp.SynthesisProcess)
                .HasForeignKey<SynthesisProcess>(x => x.ReviewProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
