using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractedDataValueConfiguration : IEntityTypeConfiguration<ExtractedDataValue>
	{
		public void Configure(EntityTypeBuilder<ExtractedDataValue> builder)
		{
			builder.ToTable("extracted_data_value");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).HasColumnName("value_id");

			builder.Property(x => x.PaperId).HasColumnName("paper_id").IsRequired();
			builder.Property(x => x.FieldId).HasColumnName("field_id").IsRequired();
			builder.Property(x => x.ReviewerId).HasColumnName("reviewer_id").IsRequired();
			builder.Property(x => x.OptionId).HasColumnName("option_id");
			builder.Property(x => x.StringValue).HasColumnName("string_value");
			builder.Property(x => x.NumericValue).HasColumnName("numeric_value").HasPrecision(18, 6);
			builder.Property(x => x.BooleanValue).HasColumnName("boolean_value");
			builder.Property(x => x.MatrixColumnId).HasColumnName("matrix_column_id");
			builder.Property(x => x.MatrixRowIndex).HasColumnName("matrix_row_index");
			builder.Property(x => x.IsConsensusFinal).HasColumnName("is_consensus_final").IsRequired().HasDefaultValue(false);
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

			// Relationships
			builder.HasOne(x => x.Paper)
				.WithMany()
				.HasForeignKey(x => x.PaperId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Field)
				.WithMany()
				.HasForeignKey(x => x.FieldId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Reviewer)
				.WithMany()
				.HasForeignKey(x => x.ReviewerId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(x => x.Option)
				.WithMany()
				.HasForeignKey(x => x.OptionId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(x => x.MatrixColumn)
				.WithMany()
				.HasForeignKey(x => x.MatrixColumnId)
				.OnDelete(DeleteBehavior.Cascade);

			// Indexes
			builder.HasIndex(x => x.PaperId);
			builder.HasIndex(x => x.FieldId);
			builder.HasIndex(x => new { x.PaperId, x.FieldId });
		}
	}
}