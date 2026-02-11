using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class PopulationConfiguration : IEntityTypeConfiguration<Population>
	{
		public void Configure(EntityTypeBuilder<Population> builder)
		{
			builder.ToTable("population");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("population_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.PicocId)
				.HasColumnName("picoc_id")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description")
				.IsRequired();

			// Configure one-to-one relationship with PicocElement
			builder.HasOne(x => x.PicocElement)
				.WithOne(x => x.Population)
				.HasForeignKey<Population>(x => x.PicocId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
