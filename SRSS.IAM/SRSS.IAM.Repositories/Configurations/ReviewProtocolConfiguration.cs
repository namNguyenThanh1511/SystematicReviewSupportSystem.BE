using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ReviewProtocolConfiguration : IEntityTypeConfiguration<ReviewProtocol>
	{
		public void Configure(EntityTypeBuilder<ReviewProtocol> builder)
		{
			builder.ToTable("review_protocol");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("protocol_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ProjectId)
				.HasColumnName("project_id")
				.IsRequired();

			builder.Property(x => x.ProtocolVersion)
				.HasColumnName("protocol_version")
				.IsRequired();

			builder.Property(x => x.Status)
				.HasColumnName("status")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at")
				.HasDefaultValueSql("CURRENT_TIMESTAMP");

			builder.Property(x => x.ApprovedAt)
				.HasColumnName("approved_at");

			builder.HasOne(x => x.Project)
				.WithMany(x => x.Protocols)
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
