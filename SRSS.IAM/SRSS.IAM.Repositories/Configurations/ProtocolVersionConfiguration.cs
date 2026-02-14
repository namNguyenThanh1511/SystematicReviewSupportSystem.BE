using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ProtocolVersionConfiguration : IEntityTypeConfiguration<ProtocolVersion>
	{
		public void Configure(EntityTypeBuilder<ProtocolVersion> builder)
		{
			builder.ToTable("protocol_version");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("version_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.VersionNumber)
				.HasColumnName("version_number")
				.IsRequired();

			builder.Property(x => x.ChangeSummary)
				.HasColumnName("change_summary");

			builder.Property(x => x.SnapshotData)
				.HasColumnName("snapshot_data");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.Versions)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}