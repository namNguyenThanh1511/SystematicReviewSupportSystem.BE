using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class DigitalLibraryConfiguration : IEntityTypeConfiguration<DigitalLibrary>
	{
		public void Configure(EntityTypeBuilder<DigitalLibrary> builder)
		{
			builder.ToTable("digital_library"); 

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("library_id")
				.IsRequired();

			builder.Property(x => x.SourceId)
				.HasColumnName("source_id")
				.IsRequired();

			builder.Property(x => x.AccessUrl)
				.HasColumnName("access_url");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Source)
				.WithOne(x => x.DigitalLibrary)
				.HasForeignKey<DigitalLibrary>(x => x.SourceId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}