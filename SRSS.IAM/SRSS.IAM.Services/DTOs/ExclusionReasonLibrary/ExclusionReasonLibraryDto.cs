namespace SRSS.IAM.Services.DTOs.ExclusionReasonLibrary
{
    public class ExclusionReasonLibraryDto
    {
        public Guid Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
