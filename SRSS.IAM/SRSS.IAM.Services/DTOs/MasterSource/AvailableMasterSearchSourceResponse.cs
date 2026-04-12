namespace SRSS.IAM.Services.DTOs.MasterSource
{
    public class AvailableMasterSearchSourceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; }
    }
}
