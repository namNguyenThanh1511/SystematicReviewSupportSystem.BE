using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class StudySelectionCriteriaConfiguration : IEntityTypeConfiguration<StudySelectionCriteria>
	{
		public void Configure(EntityTypeBuilder<StudySelectionCriteria> builder)
		{
			builder.ToTable("study_selection_criteria");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("criteria_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.SelectionCriterias)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}