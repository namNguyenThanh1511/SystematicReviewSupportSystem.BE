using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class SearchExecutionConfiguration : IEntityTypeConfiguration<SearchExecution>
    {
        public void Configure(EntityTypeBuilder<SearchExecution> builder)
        {
            builder.ToTable("search_executions");

            builder.HasKey(se => se.Id);

            builder.Property(se => se.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(se => se.IdentificationProcessId)
                .HasColumnName("identification_process_id")
                .IsRequired();

            builder.Property(se => se.SearchSourceId)
                .HasColumnName("search_source_id")
                .IsRequired();

            builder.Property(se => se.SearchQuery)
                .HasColumnName("search_query");

            builder.Property(se => se.ExecutedAt)
                .HasColumnName("executed_at")
                .IsRequired();

            builder.Property(se => se.ResultCount)
                .HasColumnName("result_count")
                .IsRequired();

            builder.Property(se => se.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(se => se.Notes)
                .HasColumnName("notes");

            builder.Property(se => se.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(se => se.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasOne(se => se.IdentificationProcess)
                .WithMany(ip => ip.SearchExecutions)
                .HasForeignKey(se => se.IdentificationProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(se => se.SearchSource)
                .WithMany()
                .HasForeignKey(se => se.SearchSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(se => se.ImportBatches)
                .WithOne(ib => ib.SearchExecution)
                .HasForeignKey(ib => ib.SearchExecutionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
