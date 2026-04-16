using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class StudyCharacteristicsConfiguration : IEntityTypeConfiguration<StudyCharacteristics>
	{
		public void Configure(EntityTypeBuilder<StudyCharacteristics> builder)
		{
			builder.ToTable("study_characteristics");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("study_characteristic_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.Language)
				.HasColumnName("language")
				.HasMaxLength(255);

			builder.Property(x => x.Domain)
				.HasColumnName("domain")
				.HasMaxLength(500);

			builder.Property(x => x.StudyType)
				.HasColumnName("study_type")
				.HasMaxLength(255);

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at")
				.HasDefaultValueSql("CURRENT_TIMESTAMP");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Protocol)
				.WithOne(p => p.StudyCharacteristics)
				.HasForeignKey<StudyCharacteristics>(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
