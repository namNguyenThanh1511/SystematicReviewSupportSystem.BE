using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class FilterSettingConfiguration : IEntityTypeConfiguration<FilterSetting>
    {
        public void Configure(EntityTypeBuilder<FilterSetting> builder)
        {
            builder.ToTable("filter_settings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(x => x.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.SearchText)
                .HasColumnName("search_text")
                .HasMaxLength(500);

            builder.Property(x => x.Keyword)
                .HasColumnName("keyword")
                .HasMaxLength(500);

            builder.Property(x => x.YearFrom)
                .HasColumnName("year_from");

            builder.Property(x => x.YearTo)
                .HasColumnName("year_to");

            builder.Property(x => x.SearchSourceId)
                .HasColumnName("search_source_id");

            builder.Property(x => x.ImportBatchId)
                .HasColumnName("import_batch_id");

            builder.Property(x => x.DoiState)
                .HasColumnName("doi_state")
                .HasMaxLength(20)
                .HasDefaultValue("all")
                .IsRequired();

            builder.Property(x => x.FullTextState)
                .HasColumnName("full_text_state")
                .HasMaxLength(20)
                .HasDefaultValue("all")
                .IsRequired();

            builder.Property(x => x.OnlyUnused)
                .HasColumnName("only_unused")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.RecentlyImported)
                .HasColumnName("recently_imported")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.FilterSettings)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ReviewProcesses)
                .WithMany(x => x.FilterSettings)
                .UsingEntity<Dictionary<string, object>>(
                    "review_process_filter_settings",
                    right => right
                        .HasOne<ReviewProcess>()
                        .WithMany()
                        .HasForeignKey("review_process_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<FilterSetting>()
                        .WithMany()
                        .HasForeignKey("filter_setting_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("review_process_filter_settings");
                        join.HasKey("review_process_id", "filter_setting_id");
                    });

            builder.HasIndex(x => x.ProjectId)
                .HasDatabaseName("idx_filter_setting_project_id");

            builder.HasIndex(x => new { x.ProjectId, x.Name })
                .HasDatabaseName("idx_filter_setting_project_name");
        }
    }
}
