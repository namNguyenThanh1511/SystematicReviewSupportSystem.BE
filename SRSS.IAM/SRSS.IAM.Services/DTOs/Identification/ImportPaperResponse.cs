namespace SRSS.IAM.Services.DTOs.Identification
{
    public class ImportPaperResponse
    {
        public int TotalImported { get; set; }
        public List<Guid> ImportedPaperIds { get; set; } = new();
    }
}
