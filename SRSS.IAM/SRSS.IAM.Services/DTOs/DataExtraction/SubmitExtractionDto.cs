using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class SubmitExtractionRequestDto
    {
        public List<ExtractedValueDto> Values { get; set; } = new();
    }

    public class ExtractedValueDto
    {
        [Required(ErrorMessage = "FieldId là bắt buộc")]
        public Guid FieldId { get; set; }

        public Guid? OptionId { get; set; }

        public string? StringValue { get; set; }

        public decimal? NumericValue { get; set; }

        public bool? BooleanValue { get; set; }

        public Guid? MatrixColumnId { get; set; }

        public int? MatrixRowIndex { get; set; }

        /// <summary>
        /// When true, signals that the data point was formally not reported in the primary study.
        /// The service will null out all value fields upon persistence.
        /// </summary>
        public bool IsNotReported { get; set; } = false;

        /// <summary>
        /// JSON-serialized array of bounding box coordinates (page, x, y, w, h) providing evidence from the PDF.
        /// </summary>
        public string? EvidenceCoordinates { get; set; }
    }
}
