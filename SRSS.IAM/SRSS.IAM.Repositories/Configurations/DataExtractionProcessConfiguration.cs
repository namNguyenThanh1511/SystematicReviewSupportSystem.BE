using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class DataExtractionProcessConfiguration : IEntityTypeConfiguration<DataExtractionProcess>
	{
		public void Configure(EntityTypeBuilder<DataExtractionProcess> builder)
		{
			builder.ToTable("data_extraction_process");

			builder.HasKey(e => e.Id);
			builder.Property(e => e.Id).HasColumnName("id").IsRequired();

			builder.Property(e => e.ReviewProcessId).HasColumnName("review_process_id").IsRequired();
			builder.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(2000);
			builder.Property(e => e.StartedAt).HasColumnName("started_at");
			builder.Property(e => e.CompletedAt).HasColumnName("completed_at");
			
			builder.Property(e => e.Status)
				.HasColumnName("status")
				.HasConversion<string>()
				.IsRequired();

			builder.Property(e => e.CreatedAt).HasColumnName("created_at");
			builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

			builder.HasOne(e => e.ReviewProcess)
				.WithMany()
				.HasForeignKey(e => e.ReviewProcessId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
