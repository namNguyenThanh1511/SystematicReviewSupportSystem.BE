using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ProtocolReviewerConfiguration : IEntityTypeConfiguration<ProtocolReviewer>
	{
		public void Configure(EntityTypeBuilder<ProtocolReviewer> builder)
		{
			builder.ToTable("protocol_reviewer");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("reviewer_id")
				.IsRequired();

			builder.Property(x => x.Name)
				.HasColumnName("name")
				.IsRequired();

			builder.Property(x => x.Role)
				.HasColumnName("role");

			builder.Property(x => x.Affiliation)
				.HasColumnName("affiliation");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");
		}
	}
}