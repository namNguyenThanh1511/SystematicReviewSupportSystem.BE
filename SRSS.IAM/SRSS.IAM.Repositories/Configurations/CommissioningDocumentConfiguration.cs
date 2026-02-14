using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class CommissioningDocumentConfiguration : IEntityTypeConfiguration<CommissioningDocument>
	{
		public void Configure(EntityTypeBuilder<CommissioningDocument> builder)
		{
			builder.ToTable("commissioning_document");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("document_id")
				.IsRequired();

			builder.Property(x => x.ProjectId)
				.HasColumnName("project_id")
				.IsRequired();

			builder.Property(x => x.Sponsor)
				.HasColumnName("sponsor");

			builder.Property(x => x.Scope)
				.HasColumnName("scope");

			builder.Property(x => x.Budget)
				.HasColumnName("budget")
				.HasColumnType("numeric");

			builder.Property(x => x.DocumentUrl)
				.HasColumnName("document_url");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Project)
				.WithMany(x => x.CommissioningDocuments)
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}