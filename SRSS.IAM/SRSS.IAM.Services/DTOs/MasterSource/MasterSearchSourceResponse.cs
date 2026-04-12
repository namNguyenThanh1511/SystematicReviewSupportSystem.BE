namespace SRSS.IAM.Services.DTOs.MasterSource
{
    public class MasterSearchSourceResponse
    {
        public Guid Id { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
