using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ChecklistItemResponseConfiguration : IEntityTypeConfiguration<ChecklistItemResponse>
    {
        public void Configure(EntityTypeBuilder<ChecklistItemResponse> builder)
        {
            builder.ToTable("checklist_item_responses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.ReviewChecklistId)
                .HasColumnName("review_checklist_id")
                .IsRequired();

            builder.Property(x => x.ItemTemplateId)
                .HasColumnName("item_template_id")
                .IsRequired();

            builder.Property(x => x.Location)
                .HasColumnName("location")
                .HasMaxLength(500);

            builder.Property(x => x.IsReported)
                .HasColumnName("is_reported")
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

            builder.HasIndex(x => new { x.ReviewChecklistId, x.ItemTemplateId })
                .IsUnique()
                .HasDatabaseName("ux_checklist_item_responses_review_item");

            builder.HasIndex(x => x.ItemTemplateId)
                .HasDatabaseName("idx_checklist_item_responses_item_template_id");
        }
    }
}
