using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class JournalConfiguration : IEntityTypeConfiguration<Journal>
	{
		public void Configure(EntityTypeBuilder<Journal> builder)
		{
			builder.ToTable("journal");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("journal_id")
				.IsRequired();

			builder.Property(x => x.SourceId)
				.HasColumnName("source_id")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Source)
				.WithOne(x => x.Journal)
				.HasForeignKey<Journal>(x => x.SourceId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}