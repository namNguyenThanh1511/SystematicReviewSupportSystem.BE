using System;
using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class AskAiFieldRequestDto
    {
        [Required]
        public Guid PaperId { get; set; }

        [Required]
        public Guid FieldId { get; set; }

        [Required]
        public string FieldName { get; set; } = null!;

        public string? FieldInstruction { get; set; }

        [Required]
        public string FieldType { get; set; } = null!;

        /// <summary>
        /// Seralized list of Valid Options for SingleSelect or MultiSelect
        /// </summary>
        public string? OptionsJson { get; set; }

        public Guid? MatrixColumnId { get; set; }

        public int? MatrixRowIndex { get; set; }
    }
}
