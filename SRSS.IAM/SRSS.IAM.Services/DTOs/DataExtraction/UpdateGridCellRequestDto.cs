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
    }
}
