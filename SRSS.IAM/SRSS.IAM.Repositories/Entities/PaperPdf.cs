using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperPdf : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public Guid PaperId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTimeOffset UploadedAt { get; set; }
        public string? FileHash { get; set; }
        public string? ExtractedDoi { get; set; }
        public PdfValidationStatus ValidationStatus { get; set; } = PdfValidationStatus.Pending;
        public PdfProcessingStatus ProcessingStatus { get; set; } = PdfProcessingStatus.Uploaded;
        public bool GrobidProcessed { get; set; }
        public bool FullTextProcessed { get; set; }
        public bool RefsExtracted { get; set; }
        public DateTimeOffset? MetadataProcessedAt { get; set; }
        public DateTimeOffset? MetadataValidatedAt { get; set; }
        public DateTimeOffset? FullTextProcessedAt { get; set; }
        public double? PageWidth { get; set; }
        public double? PageHeight { get; set; }

        public Paper? Paper { get; set; }
        public GrobidHeaderResult? GrobidHeaderResult { get; set; }
        public PaperFullText? PaperFullText { get; set; }
    }
}
