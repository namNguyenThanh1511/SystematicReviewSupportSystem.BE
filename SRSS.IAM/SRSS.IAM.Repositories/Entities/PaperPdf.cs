using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class PaperPdf : BaseEntity<Guid>
    {
        public Guid PaperId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTimeOffset UploadedAt { get; set; }
        public bool GrobidProcessed { get; set; }

        public Paper? Paper { get; set; }
        public GrobidHeaderResult? GrobidHeaderResult { get; set; }
    }
}
