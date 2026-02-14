using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class SearchTermConfiguration : IEntityTypeConfiguration<SearchTerm>
	{
		public void Configure(EntityTypeBuilder<SearchTerm> builder)
		{
			builder.ToTable("search_term");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("term_id")
				.IsRequired();

			builder.Property(x => x.Keyword)
				.HasColumnName("keyword")
				.IsRequired();

			builder.Property(x => x.Source)
				.HasColumnName("source");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");
		}
	}
}