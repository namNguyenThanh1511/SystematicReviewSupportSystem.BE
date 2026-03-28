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

            builder.Property(pc => pc.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(pc => pc.SourcePaperId)
                .HasColumnName("source_paper_id")
                .IsRequired();

            builder.Property(pc => pc.TargetPaperId)
                .HasColumnName("target_paper_id")
                .IsRequired(false);

            builder.Property(pc => pc.ReferenceEntityId)
                .HasColumnName("reference_entity_id")
                .IsRequired(false);

            builder.Property(pc => pc.ReferenceType)
                .HasColumnName("reference_type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(pc => pc.RawReference)
                .HasColumnName("raw_reference");

            builder.Property(pc => pc.ConfidenceScore)
                .HasColumnName("confidence_score");

            builder.Property(pc => pc.Source)
                .HasColumnName("source")
                .HasConversion<string>();

            builder.Property(pc => pc.IsExternal)
                .HasColumnName("is_external")
                .HasDefaultValue(false);

            builder.Property(pc => pc.ExternalId)
                .HasColumnName("external_id")
                .HasMaxLength(255);

            builder.Property(pc => pc.Weight)
                .HasColumnName("weight");

            builder.Property(pc => pc.IsLowConfidence)
                .HasColumnName("is_low_confidence")
                .HasDefaultValue(false);

            builder.Property(pc => pc.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(pc => pc.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationships
            builder.HasOne(pc => pc.SourcePaper)
                .WithMany(p => p.OutgoingCitations)
                .HasForeignKey(pc => pc.SourcePaperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pc => pc.TargetPaper)
                .WithMany(p => p.IncomingCitations)
                .HasForeignKey(pc => pc.TargetPaperId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pc => pc.ReferenceEntity)
                .WithMany(re => re.IncomingCitations)
                .HasForeignKey(pc => pc.ReferenceEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(pc => pc.SourcePaperId);
            builder.HasIndex(pc => pc.TargetPaperId);
            builder.HasIndex(pc => pc.ReferenceEntityId);
            
            // Unique mapping: Source + (TargetPaper OR ReferenceEntity)
            // Note: Since TargetPaperId and ReferenceEntityId are nullable, 
            // a single unique index won't work perfectly for both.
            // We'll rely on service-level checks or separate partial indexes if Postgres version supports it.
            // For now, keeping it simple as per requirements.
        }
    }
}
