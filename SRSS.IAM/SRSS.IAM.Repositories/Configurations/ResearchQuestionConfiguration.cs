using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ResearchQuestionConfiguration : IEntityTypeConfiguration<ResearchQuestion>
	{
		public void Configure(EntityTypeBuilder<ResearchQuestion> builder)
		{
			builder.ToTable("research_question");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id)
				.HasColumnName("research_question_id")
				.IsRequired();

			builder.Property(x => x.ProjectId)
				.HasColumnName("project_id")
				.IsRequired();

			builder.Property(x => x.QuestionTypeId)
				.HasColumnName("question_type_id")
				.IsRequired();

			builder.Property(x => x.QuestionText)
				.HasColumnName("question_text")
				.IsRequired();

			builder.Property(x => x.Rationale)
				.HasColumnName("rationale");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at")
				.HasDefaultValueSql("CURRENT_TIMESTAMP");

			builder.HasOne(x => x.Project)
				.WithMany(x => x.ResearchQuestions)
				.HasForeignKey(x => x.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.QuestionType)
				.WithMany(x => x.ResearchQuestions)
				.HasForeignKey(x => x.QuestionTypeId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
