using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ProjectTimetableConfiguration : IEntityTypeConfiguration<ProjectTimetable>
	{
		public void Configure(EntityTypeBuilder<ProjectTimetable> builder)
		{
			builder.ToTable("project_timetable");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("timetable_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Milestone)
				.HasColumnName("milestone")
				.IsRequired();

			builder.Property(x => x.PlannedDate)
				.HasColumnName("planned_date");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.Timetables)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}