namespace SRSS.IAM.Services.DTOs.ExclusionReasonLibrary
{
    public class CreateExclusionReasonRequest
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
