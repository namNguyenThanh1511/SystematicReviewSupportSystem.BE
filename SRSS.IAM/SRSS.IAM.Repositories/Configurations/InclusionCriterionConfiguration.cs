using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class InclusionCriterionConfiguration : IEntityTypeConfiguration<InclusionCriterion>
	{
		public void Configure(EntityTypeBuilder<InclusionCriterion> builder)
		{
			builder.ToTable("inclusion_criterion");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("inclusion_id")
				.IsRequired();

			builder.Property(x => x.CriteriaId)
				.HasColumnName("criteria_id")
				.IsRequired();

			builder.Property(x => x.Rule)
				.HasColumnName("rule")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Criteria)
				.WithMany(x => x.InclusionCriteria)
				.HasForeignKey(x => x.CriteriaId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}