using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class QualityCriterionConfiguration : IEntityTypeConfiguration<QualityCriterion>
	{
		public void Configure(EntityTypeBuilder<QualityCriterion> builder)
		{
			builder.ToTable("quality_criterion");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("quality_criterion_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ChecklistId)
				.HasColumnName("checklist_id")
				.IsRequired();

			builder.Property(x => x.Question)
				.HasColumnName("question")
				.IsRequired();

			builder.Property(x => x.Weight)
				.HasColumnName("weight")
				.HasColumnType("numeric");

			builder.HasOne(x => x.Checklist)
				.WithMany(x => x.Criteria)
				.HasForeignKey(x => x.ChecklistId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}