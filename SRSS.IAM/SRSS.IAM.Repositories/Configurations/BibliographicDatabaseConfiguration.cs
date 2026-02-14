using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class BibliographicDatabaseConfiguration : IEntityTypeConfiguration<BibliographicDatabase>
	{
		public void Configure(EntityTypeBuilder<BibliographicDatabase> builder)
		{
			builder.ToTable("bibliographic_database"); 

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("database_id")
				.IsRequired();

			builder.Property(x => x.SourceId)
				.HasColumnName("source_id")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Source)
				.WithOne(x => x.BibliographicDatabase)
				.HasForeignKey<BibliographicDatabase>(x => x.SourceId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}