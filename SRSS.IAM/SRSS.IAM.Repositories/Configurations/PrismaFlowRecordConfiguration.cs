using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PrismaFlowRecordConfiguration : IEntityTypeConfiguration<PrismaFlowRecord>
    {
        public void Configure(EntityTypeBuilder<PrismaFlowRecord> builder)
        {
            builder.ToTable("prisma_flow_records");

            builder.HasKey(fr => fr.Id);

            builder.Property(fr => fr.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(fr => fr.PrismaReportId)
                .HasColumnName("prisma_report_id")
                .IsRequired();

            builder.Property(fr => fr.Stage)
                .HasColumnName("stage")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(fr => fr.Label)
                .HasColumnName("label")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(fr => fr.Count)
                .HasColumnName("count")
                .IsRequired();

            builder.Property(fr => fr.Description)
                .HasColumnName("description");

            builder.Property(fr => fr.DisplayOrder)
                .HasColumnName("display_order")
                .IsRequired();

            builder.Property(fr => fr.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(fr => fr.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            // Relationship with PrismaReport
            builder.HasOne(fr => fr.PrismaReport)
                .WithMany(pr => pr.FlowRecords)
                .HasForeignKey(fr => fr.PrismaReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(fr => fr.PrismaReportId);
            builder.HasIndex(fr => fr.Stage);
        }
    }
}
