using System;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.SynthesisExecution
{
    public class ThemeEvidenceDto
    {
        public Guid Id { get; set; }
        public Guid ThemeId { get; set; }
        public Guid ExtractedDataValueId { get; set; }
        public string PaperTitle { get; set; } = string.Empty;
        public string? StringValue { get; set; }
        public decimal? NumericValue { get; set; }
        public bool? BooleanValue { get; set; }
        public Guid? OptionId { get; set; }
        public string DisplayValue { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public Guid CreatedById { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }

    public class AddEvidenceRequest
    {
        [Required]
        public Guid ExtractedDataValueId { get; set; }
        public string? Notes { get; set; }
    }
}
