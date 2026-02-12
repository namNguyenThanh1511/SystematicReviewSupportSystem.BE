using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class StudySelectionProcedureConfiguration : IEntityTypeConfiguration<StudySelectionProcedure>
	{
		public void Configure(EntityTypeBuilder<StudySelectionProcedure> builder)
		{
			builder.ToTable("study_selection_procedure");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("procedure_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Steps)
				.HasColumnName("steps")
				.IsRequired();

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.SelectionProcedures)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}