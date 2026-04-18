using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.MasterSource
{
    public class UpdateMasterSearchSourceRequest
    {
        [Required]
        [MaxLength(255)]
        public string SourceName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? BaseUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
