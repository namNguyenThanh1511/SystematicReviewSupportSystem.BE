using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ProtocolEvaluationConfiguration : IEntityTypeConfiguration<ProtocolEvaluation>
	{
		public void Configure(EntityTypeBuilder<ProtocolEvaluation> builder)
		{
			builder.ToTable("protocol_evaluation"); 

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id)
				.HasColumnName("evaluation_id")
				.IsRequired();

			builder.Property(x => x.ProtocolId)
				.HasColumnName("protocol_id")
				.IsRequired();

			builder.Property(x => x.ReviewerId)
				.HasColumnName("reviewer_id")
				.IsRequired();

			builder.Property(x => x.EvaluationResult)
				.HasColumnName("evaluation_result");

			builder.Property(x => x.Comment)
				.HasColumnName("comment");

			builder.Property(x => x.EvaluatedAt)
				.HasColumnName("evaluated_at");

			builder.Property(x => x.CreatedAt)
				.HasColumnName("created_at");

			builder.Property(x => x.ModifiedAt)
				.HasColumnName("modified_at");

			builder.HasOne(x => x.Protocol)
				.WithMany(x => x.Evaluations)
				.HasForeignKey(x => x.ProtocolId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Reviewer)
				.WithMany()
				.HasForeignKey(x => x.ReviewerId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}