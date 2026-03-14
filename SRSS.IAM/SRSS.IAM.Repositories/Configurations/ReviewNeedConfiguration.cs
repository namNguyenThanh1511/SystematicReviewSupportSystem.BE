using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ReviewNeedConfiguration : IEntityTypeConfiguration<ReviewNeed>
	{
		public void Configure(EntityTypeBuilder<ReviewNeed> builder)
		{
			builder.ToTable("review_need");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("need_id")
				.IsRequired();

			builder.Property(x => x.ProjectId)
				.HasColumnName("project_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.Property(x => x.Justification)
				.HasColumnName("justification");

			builder.Property(x => x.IdentifiedBy)
				.HasColumnName("identified_by");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Project)
				.WithMany(x => x.ReviewNeeds)
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}