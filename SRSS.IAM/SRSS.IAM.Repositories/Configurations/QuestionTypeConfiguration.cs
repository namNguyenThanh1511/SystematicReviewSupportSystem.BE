using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class QuestionTypeConfiguration : IEntityTypeConfiguration<QuestionType>
	{
		public void Configure(EntityTypeBuilder<QuestionType> builder)
		{
			builder.ToTable("question_type");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("question_type_id")
				.IsRequired();

			builder.Property(x => x.Name)
				.HasColumnName("name")
				.IsRequired();

			builder.Property(x => x.Description)
				.HasColumnName("description");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");
		}
	}
}