using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class SearchSourceConfiguration : IEntityTypeConfiguration<SearchSource>
	{
		public void Configure(EntityTypeBuilder<SearchSource> builder)
		{
			builder.ToTable("search_source");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("source_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.SourceType)
				.HasColumnName("source_type")
				.IsRequired();

			builder.Property(x => x.Name)
				.HasColumnName("name")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.SearchSources)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}