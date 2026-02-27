namespace SRSS.IAM.Services.DTOs.Identification
{
    public class CreateImportBatchRequest
    {
        public Guid SearchExecutionId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? Source { get; set; }
        public int TotalRecords { get; set; }
        public string? ImportedBy { get; set; }
    }

    public class UpdateImportBatchRequest
    {
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? Source { get; set; }
        public int? TotalRecords { get; set; }
        public string? ImportedBy { get; set; }
    }

    public class ImportBatchResponse
    {
        public Guid Id { get; set; }
        public Guid? SearchExecutionId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? Source { get; set; }
        public int TotalRecords { get; set; }
        public string? ImportedBy { get; set; }
        public DateTimeOffset ImportedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
