using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperCitationConfiguration : IEntityTypeConfiguration<PaperCitation>
    {
        public void Configure(EntityTypeBuilder<PaperCitation> builder)
        {
            builder.ToTable("paper_citations");

            builder.HasKey(pc => pc.Id);

            builder.HasOne(pc => pc.SourcePaper)
                .WithMany(p => p.OutgoingCitations)
                .HasForeignKey(pc => pc.SourcePaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pc => pc.TargetPaper)
                .WithMany(p => p.IncomingCitations)
                .HasForeignKey(pc => pc.TargetPaperId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pc => pc.SourcePaperId);
            builder.HasIndex(pc => pc.TargetPaperId);
            builder.HasIndex(pc => new { pc.SourcePaperId, pc.TargetPaperId }).IsUnique();
        }
    }
}
