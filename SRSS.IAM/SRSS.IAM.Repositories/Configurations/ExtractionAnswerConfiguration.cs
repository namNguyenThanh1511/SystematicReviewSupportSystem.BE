using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionAnswerConfiguration : IEntityTypeConfiguration<ExtractionAnswer>
	{
		public void Configure(EntityTypeBuilder<ExtractionAnswer> builder)
		{
			builder.ToTable("extraction_answer");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.FieldId).IsRequired();
			builder.Property(x => x.ValueKind).IsRequired();
			builder.Property(x => x.TextValue).HasMaxLength(4000);
			builder.Property(x => x.NumberValue).HasPrecision(18, 6);
			builder.Property(x => x.OptionIds).HasMaxLength(2000); // JSON array
			builder.Property(x => x.EvidenceQuote).HasMaxLength(2000);
			builder.Property(x => x.EvidenceSource).HasMaxLength(255);

			builder.HasOne(x => x.Field)
				.WithMany()
				.HasForeignKey(x => x.FieldId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(x => x.Draft)
				.WithMany(x => x.Answers)
				.HasForeignKey(x => x.DraftId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Submission)
				.WithMany(x => x.Answers)
				.HasForeignKey(x => x.SubmissionId)
				.OnDelete(DeleteBehavior.Cascade);

			// Index
			builder.HasIndex(x => new { x.DraftId, x.FieldId });
			builder.HasIndex(x => new { x.SubmissionId, x.FieldId });
		}
	}
}