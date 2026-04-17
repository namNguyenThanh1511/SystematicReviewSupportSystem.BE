using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class UpdateGridCellRequestDto
    {
        [Required]
        public Guid PaperId { get; set; }

        [Required]
        public Guid FieldId { get; set; }

        public Guid? MatrixColumnId { get; set; }
        public int? MatrixRowIndex { get; set; }

        public string? NewValue { get; set; }

        /// <summary>
        /// When true, the cell is set to "Not Reported". NewValue is ignored and all value fields are nulled.
        /// </summary>
        public bool IsNotReported { get; set; } = false;
    }
}
