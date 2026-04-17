using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
	public class ExtractionCommentConfiguration : IEntityTypeConfiguration<ExtractionComment>
	{
		public void Configure(EntityTypeBuilder<ExtractionComment> builder)
		{
			builder.ToTable("extraction_comment");

			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).HasColumnName("comment_id");

			builder.Property(x => x.ExtractionPaperTaskId).HasColumnName("extraction_paper_task_id").IsRequired();
			builder.Property(x => x.FieldId).HasColumnName("field_id").IsRequired();
			builder.Property(x => x.MatrixColumnId).HasColumnName("matrix_column_id");
			builder.Property(x => x.MatrixRowIndex).HasColumnName("matrix_row_index");
			builder.Property(x => x.ThreadOwnerId).HasColumnName("thread_owner_id").IsRequired();
			builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
			builder.Property(x => x.Content).HasColumnName("content").IsRequired();
			builder.Property(x => x.CreatedAt).HasColumnName("created_at");
			builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

			builder.HasOne(x => x.ExtractionPaperTask)
				.WithMany()
				.HasForeignKey(x => x.ExtractionPaperTaskId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Field)
				.WithMany()
				.HasForeignKey(x => x.FieldId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.MatrixColumn)
				.WithMany()
				.HasForeignKey(x => x.MatrixColumnId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
