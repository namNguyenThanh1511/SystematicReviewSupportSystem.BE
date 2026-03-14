using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
    {
        public void Configure(EntityTypeBuilder<ImportBatch> builder)
        {
            builder.ToTable("import_batches");

            builder.HasKey(ib => ib.Id);

            builder.Property(ib => ib.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(ib => ib.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255);

            builder.Property(ib => ib.FileType)
                .HasColumnName("file_type")
                .HasMaxLength(50);

            builder.Property(ib => ib.Source)
                .HasColumnName("source")
                .HasMaxLength(100);

            builder.Property(ib => ib.TotalRecords)
                .HasColumnName("total_records")
                .IsRequired();

            builder.Property(ib => ib.ImportedBy)
                .HasColumnName("imported_by")
                .HasMaxLength(255);

            builder.Property(ib => ib.ImportedAt)
                .HasColumnName("imported_at")
                .IsRequired();

            builder.Property(ib => ib.SearchExecutionId)
                .HasColumnName("search_execution_id");

            builder.Property(ib => ib.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(ib => ib.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // One-to-Many Relationship: ImportBatch ? Papers
            builder.HasMany(ib => ib.Papers)
                .WithOne(p => p.ImportBatch)
                .HasForeignKey(p => p.ImportBatchId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ib => ib.SearchExecution)
                .WithMany(se => se.ImportBatches)
                .HasForeignKey(ib => ib.SearchExecutionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
