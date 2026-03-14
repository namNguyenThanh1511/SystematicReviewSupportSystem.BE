using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionResolutionConfiguration : IEntityTypeConfiguration<ExtractionResolution>
	{
		public void Configure(EntityTypeBuilder<ExtractionResolution> builder)
		{
			builder.ToTable("extraction_resolution");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.StudyAssignmentId).IsRequired();
			builder.Property(x => x.FieldId).IsRequired();
			builder.Property(x => x.ResolutionType).IsRequired();
			builder.Property(x => x.ResolverUserId).IsRequired();
			builder.Property(x => x.ResolverName).IsRequired().HasMaxLength(255);
			builder.Property(x => x.ResolutionNote).HasMaxLength(2000);
			builder.Property(x => x.TextValue).HasMaxLength(4000);
			builder.Property(x => x.NumberValue).HasPrecision(18, 6);
			builder.Property(x => x.OptionIds).HasMaxLength(2000); // JSON array
			builder.Property(x => x.ResolvedAt).HasDefaultValueSql("now() at time zone 'utc'");

			builder.HasOne(x => x.StudyAssignment)
				.WithMany(x => x.Resolutions)
				.HasForeignKey(x => x.StudyAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Field)
				.WithMany()
				.HasForeignKey(x => x.FieldId)
				.OnDelete(DeleteBehavior.Restrict);

			// Index
			builder.HasIndex(x => new { x.StudyAssignmentId, x.FieldId }).IsUnique();
		}
	}
}