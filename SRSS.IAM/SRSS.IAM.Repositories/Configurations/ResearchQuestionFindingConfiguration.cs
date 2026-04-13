using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class ResearchQuestionFindingConfiguration : IEntityTypeConfiguration<ResearchQuestionFinding>
    {
        public void Configure(EntityTypeBuilder<ResearchQuestionFinding> builder)
        {
            builder.ToTable("research_question_findings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("finding_id")
                .IsRequired();

            builder.Property(x => x.SynthesisProcessId)
                .HasColumnName("synthesis_process_id")
                .IsRequired();

            builder.Property(x => x.ResearchQuestionId)
                .HasColumnName("research_question_id")
                .IsRequired();

            builder.Property(x => x.AnswerText)
                .HasColumnName("answer_text")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.AuthorId)
                .HasColumnName("author_id")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasOne(x => x.SynthesisProcess)
                .WithMany(x => x.Findings)
                .HasForeignKey(x => x.SynthesisProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ResearchQuestion)
                .WithMany()
                .HasForeignKey(x => x.ResearchQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
