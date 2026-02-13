using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ContextConfiguration : IEntityTypeConfiguration<Context>
	{
		public void Configure(EntityTypeBuilder<Context> builder)
		{
			builder.ToTable("context");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("context_id")
				.IsRequired();

			builder.Property(x => x.PicocId)
				.HasColumnName("picoc_id")
				.IsRequired();

			builder.Property(x => x.Environment)
				.HasColumnName("environment");

			builder.Property(x => x.Description)
				.HasColumnName("description")
				.IsRequired();

			// Configure one-to-one relationship with PicocElement
			builder.HasOne(x => x.PicocElement)
				.WithOne(x => x.Context)
				.HasForeignKey<Context>(x => x.PicocId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}