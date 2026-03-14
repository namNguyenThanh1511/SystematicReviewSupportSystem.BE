namespace SRSS.IAM.Services.DTOs.Identification
{
    public class RisImportResultDto
    {
        public Guid? ImportBatchId { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedRecords { get; set; }
        public int DuplicateRecords { get; set; }
        public int SkippedRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<Guid> ImportedPaperIds { get; set; } = new();
    }
}

