using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ReviewObjectiveConfiguration : IEntityTypeConfiguration<ReviewObjective>
	{
		public void Configure(EntityTypeBuilder<ReviewObjective> builder)
		{
			builder.ToTable("review_objective");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("objective_id")
				.IsRequired();

			builder.Property(x => x.ProjectId)
				.HasColumnName("project_id")
				.IsRequired();

			builder.Property(x => x.ObjectiveStatement)
				.HasColumnName("objective_statement");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Project)
				.WithMany(x => x.ReviewObjectives)
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}