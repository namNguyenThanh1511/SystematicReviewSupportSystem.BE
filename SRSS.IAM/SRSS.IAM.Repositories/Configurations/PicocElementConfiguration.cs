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
	public class PicocElementConfiguration : IEntityTypeConfiguration<PicocElement>
	{
		public void Configure(EntityTypeBuilder<PicocElement> builder)
		{
			builder.ToTable("picoc_element");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("picoc_id")
				.HasDefaultValueSql("uuid_generate_v4()");

			builder.Property(x => x.ResearchQuestionId)
				.HasColumnName("research_question_id")
				.IsRequired();

			builder.Property(x => x.ElementType)
				.HasColumnName("element_type")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description")
				.IsRequired();

			builder.HasOne(x => x.ResearchQuestion)
				.WithMany(x => x.PicocElements)
				.HasForeignKey(x => x.ResearchQuestionId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(p => p.Comparison)     
				.WithOne(c => c.PicocElement)
				.HasForeignKey<Comparison>(c => c.PicocId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}