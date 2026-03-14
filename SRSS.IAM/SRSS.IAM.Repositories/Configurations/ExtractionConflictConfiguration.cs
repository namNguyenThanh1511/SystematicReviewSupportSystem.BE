using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionConflictConfiguration : IEntityTypeConfiguration<ExtractionConflict>
	{
		public void Configure(EntityTypeBuilder<ExtractionConflict> builder)
		{
			builder.ToTable("extraction_conflict");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.StudyAssignmentId).IsRequired();
			builder.Property(x => x.FieldId).IsRequired();
			builder.Property(x => x.SubmissionAId).IsRequired();
			builder.Property(x => x.SubmissionBId).IsRequired();
			builder.Property(x => x.IsResolved).HasDefaultValue(false);

			builder.HasOne(x => x.StudyAssignment)
				.WithMany(x => x.Conflicts)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Field)
				.WithMany()
				.HasForeignKey(x => x.FieldId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(x => x.SubmissionA)
				.WithMany()
				.HasForeignKey(x => x.SubmissionAId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.SubmissionB)
				.WithMany()
				.HasForeignKey(x => x.SubmissionBId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Resolution)
				.WithMany()
				.HasForeignKey(x => x.ResolutionId)
				.OnDelete(DeleteBehavior.SetNull);

			// Index
			builder.HasIndex(x => new { x.StudyAssignmentId, x.IsResolved });
		}
	}
}