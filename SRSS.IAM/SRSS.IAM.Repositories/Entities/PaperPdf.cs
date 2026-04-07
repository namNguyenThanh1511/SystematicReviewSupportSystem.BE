using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperPdf : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public Guid PaperId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTimeOffset UploadedAt { get; set; }
        public bool GrobidProcessed { get; set; }

        public string? FileHash { get; set; } // Thêm cột này (SHA-256)
        public bool FullTextProcessed { get; set; } // Đánh dấu đã trích xuất full-text thành công chưa
        public bool RefsExtracted { get; set; } // Đánh dấu đã chạy Extract Refs thành công chưa

        public Paper? Paper { get; set; }
        public GrobidHeaderResult? GrobidHeaderResult { get; set; }
        public PaperFullText? PaperFullText { get; set; }
    }
}
