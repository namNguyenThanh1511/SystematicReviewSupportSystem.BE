using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionSubmissionConfiguration : IEntityTypeConfiguration<ExtractionSubmission>
	{
		public void Configure(EntityTypeBuilder<ExtractionSubmission> builder)
		{
			builder.ToTable("extraction_submission");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.StudyAssignmentId).IsRequired();
			builder.Property(x => x.TemplateId).IsRequired();
			builder.Property(x => x.ReviewerUserId).IsRequired();
			builder.Property(x => x.ReviewerName).IsRequired().HasMaxLength(255);
			builder.Property(x => x.SubmissionNote).HasMaxLength(2000);
			builder.Property(x => x.SubmittedAt).HasDefaultValueSql("now() at time zone 'utc'");

			builder.HasOne(x => x.StudyAssignment)
				.WithMany(x => x.Submissions)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Template)
				.WithMany()
				.HasForeignKey(x => x.TemplateId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(x => x.Answers)
				.WithOne(x => x.Submission)
				.HasForeignKey(x => x.SubmissionId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}