using System;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.SynthesisExecution
{
    public class SynthesisThemeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ColorCode { get; set; }
        public Guid CreatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public System.Collections.Generic.List<ThemeEvidenceDto> Evidences { get; set; } = new();
    }

    public class CreateThemeRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [MaxLength(50)]
        public string? ColorCode { get; set; }
    }

    public class UpdateThemeRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [MaxLength(50)]
        public string? ColorCode { get; set; }
    }
}
