using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ReviewChecklistConfiguration : IEntityTypeConfiguration<ReviewChecklist>
    {
        public void Configure(EntityTypeBuilder<ReviewChecklist> builder)
        {
            builder.ToTable("review_checklists");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(x => x.TemplateId)
                .HasColumnName("template_id")
                .IsRequired();

            builder.Property(x => x.IsCompleted)
                .HasColumnName("is_completed")
                .IsRequired();

            builder.Property(x => x.CompletionPercentage)
                .HasColumnName("completion_percentage")
                .IsRequired();

            builder.Property(x => x.LastUpdatedAt)
                .HasColumnName("last_updated_at")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ReviewChecklists)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ItemResponses)
                .WithOne(x => x.ReviewChecklist)
                .HasForeignKey(x => x.ReviewChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ProjectId)
                 .HasDatabaseName("idx_review_checklists_project_id");

            builder.HasIndex(x => new { x.ProjectId, x.TemplateId })
                .IsUnique()
                .HasDatabaseName("ux_review_checklists_project_template");
        }
    }
}
