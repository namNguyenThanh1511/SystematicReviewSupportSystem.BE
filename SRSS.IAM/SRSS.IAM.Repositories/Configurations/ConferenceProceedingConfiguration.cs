using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ConferenceProceedingConfiguration : IEntityTypeConfiguration<ConferenceProceeding>
	{
		public void Configure(EntityTypeBuilder<ConferenceProceeding> builder)
		{
			builder.ToTable("conference_proceeding");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("conference_id")
				.IsRequired();

			builder.Property(x => x.SourceId)
				.HasColumnName("source_id")
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Source)
				.WithOne(x => x.ConferenceProceeding)
				.HasForeignKey<ConferenceProceeding>(x => x.SourceId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}