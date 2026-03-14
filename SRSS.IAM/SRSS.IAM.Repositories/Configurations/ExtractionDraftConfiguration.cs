using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionDraftConfiguration : IEntityTypeConfiguration<ExtractionDraft>
	{
		public void Configure(EntityTypeBuilder<ExtractionDraft> builder)
		{
			builder.ToTable("extraction_draft");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.StudyAssignmentId).IsRequired();
			builder.Property(x => x.TemplateId).IsRequired();
			builder.Property(x => x.ReviewerUserId).IsRequired();
			builder.Property(x => x.ReviewerName).IsRequired().HasMaxLength(255);

			builder.Property(x => x.DraftVersion).HasDefaultValue(1);
			builder.Property(x => x.IsAutosave).HasDefaultValue(true);

			builder.HasOne(x => x.StudyAssignment)
				.WithMany(x => x.Drafts)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Template)
				.WithMany()
				.HasForeignKey(x => x.TemplateId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(x => x.Answers)
				.WithOne(x => x.Draft)
				.HasForeignKey(x => x.DraftId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}