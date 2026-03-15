using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperSourceMetadataConfiguration : IEntityTypeConfiguration<PaperSourceMetadata>
    {
        public void Configure(EntityTypeBuilder<PaperSourceMetadata> builder)
        {
            builder.ToTable("paper_source_metadatas");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Paper)
                   .WithMany(p => p.SourceMetadatas)
                   .HasForeignKey(x => x.PaperId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
