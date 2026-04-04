namespace SRSS.IAM.Services.DTOs.Identification
{
    public class MarkAsDuplicateRequest
    {
        public Guid PaperId { get; set; }
        public Guid DuplicateOfPaperId { get; set; }
        public string? Reason { get; set; }
    }
}