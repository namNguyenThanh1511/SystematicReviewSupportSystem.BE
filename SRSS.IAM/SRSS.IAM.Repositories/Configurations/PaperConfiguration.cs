using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

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
                .IsRequired();

            builder.Property(p => p.Authors)
                .HasColumnName("authors");

            builder.Property(p => p.Abstract)
                .HasColumnName("abstract");

            builder.Property(p => p.DOI)
                .HasColumnName("doi");


            builder.HasIndex(p => p.DOI);

            builder.Property(p => p.PublicationYear)
                .HasColumnName("publication_year");

            builder.Property(p => p.PublicationDate)
                .HasColumnName("publication_date");

            builder.Property(p => p.Journal)
                .HasColumnName("journal");

            builder.Property(p => p.JournalIssn)
                .HasColumnName("journal_issn");

            builder.Property(p => p.Volume)
                .HasColumnName("volume");

            builder.Property(p => p.Issue)
                .HasColumnName("issue");

            builder.Property(p => p.Pages)
                .HasColumnName("pages");

            builder.Property(p => p.Publisher)
                .HasColumnName("publisher");

            builder.Property(p => p.ConferenceLocation)
                .HasColumnName("conference_location");

            builder.Property(p => p.ConferenceName)
                .HasColumnName("conference_name");

            builder.Property(p => p.PublicationType)
                .HasColumnName("publication_type");

            builder.Property(p => p.Url)
                .HasColumnName("url");

            builder.Property(p => p.Keywords)
                .HasColumnName("keywords");

            builder.Property(p => p.RawReference)
                .HasColumnName("raw_reference");

            // ============================================
            // NEW FIELDS - LANGUAGE & METADATA
            // ============================================
            builder.Property(p => p.Language)
                .HasColumnName("language");

            builder.Property(p => p.PublicationYearInt)
                .HasColumnName("publication_year_int");

            builder.Property(p => p.AbstractLanguage)
                .HasColumnName("abstract_language");

            // ============================================
            // NEW FIELDS - EXTENDED CONFERENCE METADATA
            // ============================================
            builder.Property(p => p.ConferenceCountry)
                .HasColumnName("conference_country");

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
                .HasColumnName("journal_e_issn");

            builder.Property(p => p.JournalPublisher)
                .HasColumnName("journal_publisher");

            // ============================================
            // NEW FIELDS - IMPORT TRACEABILITY
            // ============================================
            builder.Property(p => p.Source)
                .HasColumnName("source");

            builder.Property(p => p.SourceRecordId)
                .HasColumnName("source_record_id");

            builder.Property(p => p.ImportBatchId)
                .HasColumnName("import_batch_id");

            builder.Property(p => p.CreatedFromImportBatchId)
                .HasColumnName("created_from_import_batch_id");

            builder.Property(p => p.SearchSourceId)
                .HasColumnName("search_source_id");

            builder.HasIndex(p => p.SearchSourceId);

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

            builder.Property(p => p.FullTextRetrievalStatus)
                .HasColumnName("full_text_retrieval_status")
                .HasDefaultValue(FullTextRetrievalStatus.Unknown);

            builder.Property(p => p.FullTextAvailable)
                .HasColumnName("full_text_available");

            builder.Property(p => p.AccessType)
                .HasColumnName("access_type")
                .HasConversion<string>();

            // ============================================
            // AUDIT FIELDS
            // ============================================
            builder.Property(p => p.InternalNotes)
                .HasColumnName("internal_notes");

            builder.Property(p => p.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            // ============================================
            // EXTERNAL METADATA (OPENALEX)
            // ============================================
            builder.Property(p => p.OpenAlexId)
                .HasColumnName("open_alex_id")
                .HasMaxLength(255);

            builder.Property(p => p.ExternalCitationCount)
                .HasColumnName("external_citation_count");

            builder.Property(p => p.ExternalReferenceCount)
                .HasColumnName("external_reference_count");

            builder.Property(p => p.ExternalCitedByPercentile)
                .HasColumnName("external_cited_by_percentile");

            builder.Property(p => p.ExternalLastFetchedAt)
                .HasColumnName("external_last_fetched_at");

            builder.Property(p => p.ExternalSource)
                .HasColumnName("external_source")
                .HasMaxLength(100);

            builder.Property(p => p.ExternalDataFetched)
                .HasColumnName("external_data_fetched")
                .HasDefaultValue(false);

            //project
            builder.Property(p => p.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();



            // ============================================
            // BASE ENTITY FIELDS
            // ============================================
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(p => p.ModifiedAt)
                .HasColumnName("modified_at")
                .IsRequired();

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}


