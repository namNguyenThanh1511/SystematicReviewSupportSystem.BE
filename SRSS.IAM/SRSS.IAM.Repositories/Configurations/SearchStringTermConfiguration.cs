using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class SearchStringTermConfiguration : IEntityTypeConfiguration<SearchStringTerm>
	{
		public void Configure(EntityTypeBuilder<SearchStringTerm> builder)
		{
			builder.ToTable("search_string_term");

			builder.HasKey(x => new { x.SearchStringId, x.TermId });

			builder.Property(x => x.SearchStringId)
				.HasColumnName("search_string_id");

			builder.Property(x => x.TermId)
				.HasColumnName("term_id");

			builder.HasOne(x => x.SearchString)
				.WithMany(x => x.SearchStringTerms)
				.HasForeignKey(x => x.SearchStringId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.SearchTerm)
				.WithMany(x => x.SearchStringTerms)
				.HasForeignKey(x => x.TermId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}