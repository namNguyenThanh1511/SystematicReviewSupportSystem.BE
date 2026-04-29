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
        public string FieldName { get; set; } = string.Empty;
        public string? StringValue { get; set; }

        private decimal? _numericValue;
        public decimal? NumericValue 
        { 
            get => _numericValue; 
            set => _numericValue = value.HasValue ? value.Value / 1.000000000000000000000000000000000m : null;
        }

        public bool? BooleanValue { get; set; }
        public Guid? OptionId { get; set; }
        public string DisplayValue { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal? QaScore { get; set; }
        public bool IsHighQuality { get; set; }
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
