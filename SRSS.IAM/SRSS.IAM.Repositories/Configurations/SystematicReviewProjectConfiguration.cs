using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class SystematicReviewProjectConfiguration : IEntityTypeConfiguration<SystematicReviewProject>
	{
		public void Configure(EntityTypeBuilder<SystematicReviewProject> builder)
		{
			builder.ToTable("systematic_review_project");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("project_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.Title)
				.HasColumnName("title")
				.IsRequired();

			builder.Property(x => x.Domain)
				.HasColumnName("domain");

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.Property(x => x.Status)
				.HasColumnName("status")
				.IsRequired();

			builder.Property(x => x.StartDate)
				.HasColumnName("start_date");

			builder.Property(x => x.EndDate)
				.HasColumnName("end_date");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at")
				.HasDefaultValueSql("CURRENT_TIMESTAMP");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at")
				.HasDefaultValueSql("CURRENT_TIMESTAMP");
		}
	}
}