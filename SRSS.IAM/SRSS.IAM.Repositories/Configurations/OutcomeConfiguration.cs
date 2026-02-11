using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class OutcomeConfiguration : IEntityTypeConfiguration<Outcome>
	{
		public void Configure(EntityTypeBuilder<Outcome> builder)
		{
			builder.ToTable("outcome");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("outcome_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.PicocId)
				.HasColumnName("picoc_id")
				.IsRequired();

			builder.Property(x => x.Metric)
				.HasColumnName("metric");

			builder.Property(x => x.Description)
				.HasColumnName("description")
				.IsRequired();

			// Configure one-to-one relationship with PicocElement
			builder.HasOne(x => x.PicocElement)
				.WithOne(x => x.Outcome)
				.HasForeignKey<Outcome>(x => x.PicocId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
