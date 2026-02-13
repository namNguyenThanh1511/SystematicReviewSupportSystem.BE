using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class InterventionConfiguration : IEntityTypeConfiguration<Intervention>
	{
		public void Configure(EntityTypeBuilder<Intervention> builder)
		{
			builder.ToTable("intervention");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("intervention_id")
				.IsRequired();

			builder.Property(x => x.PicocId)
				.HasColumnName("picoc_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description")
				.IsRequired();

			// Configure one-to-one relationship with PicocElement
			builder.HasOne(x => x.PicocElement)
				.WithOne(x => x.Intervention)
				.HasForeignKey<Intervention>(x => x.PicocId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
