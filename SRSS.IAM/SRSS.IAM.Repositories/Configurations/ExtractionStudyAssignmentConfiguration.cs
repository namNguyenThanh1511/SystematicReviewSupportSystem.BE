using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionStudyAssignmentConfiguration : IEntityTypeConfiguration<ExtractionStudyAssignment>
	{
		public void Configure(EntityTypeBuilder<ExtractionStudyAssignment> builder)
		{
			builder.ToTable("extraction_study_assignment");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.ExtractionContextId).IsRequired();
			builder.Property(x => x.PaperId).IsRequired();

			builder.Property(x => x.Status)
				.HasDefaultValue(ExtractionStudyStatusEnum.ToDo);

			builder.Property(x => x.ConflictCount)
				.HasDefaultValue(0);

			// Relationships
			builder.HasOne(x => x.ExtractionContext)
				.WithMany(x => x.StudyAssignments)
				.HasForeignKey(x => x.ExtractionContextId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Paper)
				.WithMany()
				.HasForeignKey(x => x.PaperId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Drafts)
				.WithOne(x => x.StudyAssignment)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Submissions)
				.WithOne(x => x.StudyAssignment)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Conflicts)
				.WithOne(x => x.StudyAssignment)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Resolutions)
				.WithOne(x => x.StudyAssignment)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			// Unique constraint: 1 paper per context
			builder.HasIndex(x => new { x.ExtractionContextId, x.PaperId }).IsUnique();
		}
	}
}