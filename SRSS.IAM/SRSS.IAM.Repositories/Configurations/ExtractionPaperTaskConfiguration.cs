using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionPaperTaskConfiguration : IEntityTypeConfiguration<ExtractionPaperTask>
	{
		public void Configure(EntityTypeBuilder<ExtractionPaperTask> builder)
		{
			builder.ToTable("extraction_paper_task");

			builder.HasKey(e => e.Id);
			builder.Property(e => e.Id).HasColumnName("id").IsRequired();

			builder.Property(e => e.DataExtractionProcessId).HasColumnName("data_extraction_process_id").IsRequired();
			builder.Property(e => e.PaperId).HasColumnName("paper_id").IsRequired();
			builder.Property(e => e.Reviewer1Id).HasColumnName("reviewer_1_id");
			builder.Property(e => e.Reviewer2Id).HasColumnName("reviewer_2_id");
			builder.Property(e => e.AdjudicatorId).HasColumnName("adjudicator_id");

			builder.Property(e => e.Reviewer1Status)
				.HasColumnName("reviewer_1_status")
				.HasConversion<string>()
				.IsRequired();

			builder.Property(e => e.Reviewer2Status)
				.HasColumnName("reviewer_2_status")
				.HasConversion<string>()
				.IsRequired();

			builder.Property(e => e.Status)
				.HasColumnName("status")
				.HasConversion<string>()
				.IsRequired();

			builder.Property(e => e.CreatedAt).HasColumnName("created_at");
			builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

			builder.HasOne(e => e.DataExtractionProcess)
				.WithMany(d => d.ExtractionPaperTasks)
				.HasForeignKey(e => e.DataExtractionProcessId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(e => e.Paper)
				.WithMany()
				.HasForeignKey(e => e.PaperId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(e => e.Reviewer1)
				.WithMany()
				.HasForeignKey(e => e.Reviewer1Id)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Reviewer2)
				.WithMany()
				.HasForeignKey(e => e.Reviewer2Id)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Adjudicator)
				.WithMany()
				.HasForeignKey(e => e.AdjudicatorId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
