using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.Configurations
{
    public class PaperConfiguration : IEntityTypeConfiguration<Paper>
    {
        public void Configure(EntityTypeBuilder<Paper> builder)
        {
            builder.ToTable("papers");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .IsRequired();

            // ============================================
            // EXISTING CORE FIELDS
            // ============================================
            builder.Property(p => p.Title)
                .HasColumnName("title")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(p => p.Authors)
                .HasColumnName("authors")
                .HasMaxLength(1000);

            builder.Property(p => p.Abstract)
                .HasColumnName("abstract");

            builder.Property(p => p.DOI)
                .HasColumnName("doi")
                .HasMaxLength(255);

            builder.HasIndex(p => p.DOI);

            builder.Property(p => p.PublicationYear)
                .HasColumnName("publication_year")
                .HasMaxLength(10);

            builder.Property(p => p.PublicationDate)
                .HasColumnName("publication_date");

            builder.Property(p => p.Journal)
                .HasColumnName("journal")
                .HasMaxLength(255);

            builder.Property(p => p.JournalIssn)
                .HasColumnName("journal_issn")
                .HasMaxLength(50);

            builder.Property(p => p.Volume)
                .HasColumnName("volume")
                .HasMaxLength(50);

            builder.Property(p => p.Issue)
                .HasColumnName("issue")
                .HasMaxLength(50);

            builder.Property(p => p.Pages)
                .HasColumnName("pages")
                .HasMaxLength(100);

            builder.Property(p => p.Publisher)
                .HasColumnName("publisher")
                .HasMaxLength(255);

            builder.Property(p => p.ConferenceLocation)
                .HasColumnName("conference_location")
                .HasMaxLength(255);

            builder.Property(p => p.ConferenceName)
                .HasColumnName("conference_name")
                .HasMaxLength(255);

            builder.Property(p => p.PublicationType)
                .HasColumnName("publication_type")
                .HasMaxLength(50);

            builder.Property(p => p.Url)
                .HasColumnName("url")
                .HasMaxLength(500);

            builder.Property(p => p.Keywords)
                .HasColumnName("keywords");

            builder.Property(p => p.RawReference)
                .HasColumnName("raw_reference");

            builder.Property(p => p.CurrentSelectionStatus)
                .HasColumnName("current_selection_status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.IsIncludedFinal)
                .HasColumnName("is_included_final")
                .HasDefaultValue(false)
                .IsRequired();
            // ============================================
            // NEW FIELDS - LANGUAGE & METADATA
            // ============================================
            builder.Property(p => p.Language)
                .HasColumnName("language")
                .HasMaxLength(50);

            builder.Property(p => p.PublicationYearInt)
                .HasColumnName("publication_year_int");

            builder.Property(p => p.AbstractLanguage)
                .HasColumnName("abstract_language")
                .HasMaxLength(50);

            // ============================================
            // NEW FIELDS - EXTENDED CONFERENCE METADATA
            // ============================================
            builder.Property(p => p.ConferenceCountry)
                .HasColumnName("conference_country")
                .HasMaxLength(100);

            builder.Property(p => p.ConferenceStartDate)
                .HasColumnName("conference_start_date");

            builder.Property(p => p.ConferenceEndDate)
                .HasColumnName("conference_end_date");

            builder.Property(p => p.ConferenceYear)
                .HasColumnName("conference_year");

            // ============================================
            // NEW FIELDS - EXTENDED JOURNAL METADATA
            // ============================================
            builder.Property(p => p.JournalEIssn)
                .HasColumnName("journal_e_issn")
                .HasMaxLength(50);

            builder.Property(p => p.JournalPublisher)
                .HasColumnName("journal_publisher")
                .HasMaxLength(255);

            // ============================================
            // NEW FIELDS - IMPORT TRACEABILITY
            // ============================================
            builder.Property(p => p.Source)
                .HasColumnName("source")
                .HasMaxLength(100);

            builder.Property(p => p.SourceRecordId)
                .HasColumnName("source_record_id")
                .HasMaxLength(255);

            builder.Property(p => p.ImportBatchId)
                .HasColumnName("import_batch_id");

            builder.Property(p => p.ImportedAt)
                .HasColumnName("imported_at");

            builder.Property(p => p.ImportedBy)
                .HasColumnName("imported_by")
                .HasMaxLength(255);

            // ============================================
            // NEW FIELDS - FULL TEXT ACCESS
            // ============================================
            builder.Property(p => p.PdfUrl)
                .HasColumnName("pdf_url")
                .HasMaxLength(500);

            builder.Property(p => p.FullTextAvailable)
                .HasColumnName("full_text_available");

            builder.Property(p => p.AccessType)
                .HasColumnName("access_type")
                .HasConversion<string>();

            // ============================================
            // NEW FIELDS - SELECTION WORKFLOW
            // ============================================
            builder.Property(p => p.LastDecisionAt)
                .HasColumnName("last_decision_at");

            builder.Property(p => p.InternalNotes)
                .HasColumnName("internal_notes");

            // ============================================
            // BASE ENTITY FIELDS
            // ============================================
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(p => p.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();
        }
    }
}


