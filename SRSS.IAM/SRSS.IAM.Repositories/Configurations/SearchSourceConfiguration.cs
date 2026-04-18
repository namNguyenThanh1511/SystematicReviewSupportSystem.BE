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
				.HasColumnName("id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.MasterSourceId)
				.HasColumnName("master_source_id");

			builder.Property(x => x.Name)
				.HasColumnName("name")
				.HasMaxLength(255)
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at")
				.IsRequired();

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at")
				.IsRequired();

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.SearchSources)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.MasterSource)
				.WithMany()
				.HasForeignKey(x => x.MasterSourceId)
				.OnDelete(DeleteBehavior.SetNull);
		}
	}
}