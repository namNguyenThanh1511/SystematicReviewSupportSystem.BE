using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_logs");

            builder.Property(a => a.Id).HasColumnName("id");
            builder.Property(a => a.UserId).HasColumnName("user_id");
            builder.Property(a => a.UserName).HasColumnName("user_name");
            builder.Property(a => a.Action).HasColumnName("action");
            builder.Property(a => a.ActionType).HasColumnName("action_type");
            builder.Property(a => a.ResourceType).HasColumnName("resource_type");
            builder.Property(a => a.ResourceId).HasColumnName("resource_id");
            builder.Property(a => a.Status).HasColumnName("status");
            builder.Property(a => a.IpAddress).HasColumnName("ip_address");
            builder.Property(a => a.UserAgent).HasColumnName("user_agent");
            builder.Property(a => a.Importance).HasColumnName("importance");
            builder.Property(a => a.OldValue).HasColumnName("old_value");
            builder.Property(a => a.NewValue).HasColumnName("new_value");
            builder.Property(a => a.AffectedColumns).HasColumnName("affected_columns");
            builder.Property(a => a.Timestamp).HasColumnName("timestamp");
            builder.Property(a => a.ProjectId).HasColumnName("project_id");
            builder.Property(a => a.CreatedAt).HasColumnName("created_at");
            builder.Property(a => a.ModifiedAt).HasColumnName("modified_at");
        }
    }
}