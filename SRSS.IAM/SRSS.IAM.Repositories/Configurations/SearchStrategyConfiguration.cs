using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class SearchStrategyConfiguration : IEntityTypeConfiguration<SearchStrategy>
	{
		public void Configure(EntityTypeBuilder<SearchStrategy> builder)
		{
			builder.ToTable("search_strategy");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("id")
				.IsRequired();

			builder.Property(x => x.SearchSourceId)
				.HasColumnName("search_source_id")
				.IsRequired();

			builder.Property(x => x.Query)
				.HasColumnName("query")
				.IsRequired();

			builder.Property(x => x.Fields)
				.HasColumnName("fields")
				.HasColumnType("text[]");

			builder.Property(x => x.PopulationKeywords)
				.HasColumnName("population_keywords")
				.HasColumnType("text[]");

			builder.Property(x => x.InterventionKeywords)
				.HasColumnName("intervention_keywords")
				.HasColumnType("text[]");

			builder.Property(x => x.ComparisonKeywords)
				.HasColumnName("comparison_keywords")
				.HasColumnType("text[]");

			builder.Property(x => x.OutcomeKeywords)
				.HasColumnName("outcome_keywords")
				.HasColumnType("text[]");

			builder.Property(x => x.ContextKeywords)
				.HasColumnName("context_keywords")
				.HasColumnType("text[]");

			builder.Property(x => x.DateSearched)
				.HasColumnName("date_searched");

			builder.Property(x => x.Version)
				.HasColumnName("version")
				.HasMaxLength(50);

			builder.Property(x => x.Notes)
				.HasColumnName("notes");

			builder.Property(x => x.FiltersJson)
				.HasColumnName("filters_json")
				.HasColumnType("jsonb");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at")
				.IsRequired();

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at")
				.IsRequired();

			builder.HasOne(x => x.SearchSource)
				.WithMany(x => x.Strategies)
				.HasForeignKey(x => x.SearchSourceId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
