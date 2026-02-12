using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class QualityChecklistConfiguration : IEntityTypeConfiguration<QualityChecklist>
	{
		public void Configure(EntityTypeBuilder<QualityChecklist> builder)
		{
			builder.ToTable("quality_checklist");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("checklist_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.QaStrategyId)
				.HasColumnName("qa_strategy_id")
				.IsRequired();

			builder.Property(x => x.Name)
				.HasColumnName("name")
				.IsRequired();

			builder.HasOne(x => x.QaStrategy)
				.WithMany(x => x.Checklists)
				.HasForeignKey(x => x.QaStrategyId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}