using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionContextConfiguration : IEntityTypeConfiguration<ExtractionContext>
	{
		public void Configure(EntityTypeBuilder<ExtractionContext> builder)
		{
			builder.ToTable("extraction_context");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.ReviewProcessId)
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.IsRequired();

			builder.Property(x => x.Mode)
				.HasDefaultValue(ExtractionModeEnum.SingleExtraction);

			builder.Property(x => x.DoubleBlind)
				.HasDefaultValue(true);

			builder.Property(x => x.AutoSaveIntervalSeconds)
				.HasDefaultValue(30);

			builder.Property(x => x.IsCompleted)
				.HasDefaultValue(false);

			// Relationships
			builder.HasOne(x => x.ReviewProcess)
				.WithMany()
				.HasForeignKey(x => x.ReviewProcessId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.DefaultTemplate)
				.WithMany()
				.HasForeignKey(x => x.DefaultTemplateId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasMany(x => x.StudyAssignments)
				.WithOne(x => x.ExtractionContext)
				.HasForeignKey(x => x.ExtractionContextId)
				.OnDelete(DeleteBehavior.Cascade);

			// Index
			builder.HasIndex(x => x.ReviewProcessId).IsUnique();
		}
	}
}