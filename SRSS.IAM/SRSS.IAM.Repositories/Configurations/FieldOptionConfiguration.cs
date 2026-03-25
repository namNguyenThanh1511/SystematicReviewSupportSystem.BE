using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class FieldOptionConfiguration : IEntityTypeConfiguration<FieldOption>
    {
        public void Configure(EntityTypeBuilder<FieldOption> builder)
        {
            builder.ToTable("field_option");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FieldId).IsRequired();
            builder.Property(x => x.Value).IsRequired().HasMaxLength(500);
            builder.Property(x => x.DisplayOrder).HasDefaultValue(0);

            builder.HasOne(x => x.Field)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.FieldId);
        }
    }
}