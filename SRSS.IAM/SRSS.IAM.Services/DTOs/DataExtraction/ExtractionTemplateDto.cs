using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ExtractionTemplateDto
    {
        public Guid? TemplateId { get; set; }

        [Required(ErrorMessage = "DataExtractionProcessId là bắt buộc")]
        public Guid DataExtractionProcessId { get; set; }

        [Required(ErrorMessage = "Name là bắt buộc")]
        [StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách sections (mỗi section chứa fields)
        /// </summary>
        public List<ExtractionSectionDto> Sections { get; set; } = new();
    }

    public class ExtractionSectionDto
    {
        public Guid? SectionId { get; set; }

        [Required(ErrorMessage = "Section Name là bắt buộc")]
        [StringLength(500, ErrorMessage = "Section Name không được vượt quá 500 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Section Type: 0=FlatForm, 1=MatrixGrid
        /// </summary>
        public int SectionType { get; set; }

        public int OrderIndex { get; set; }

        public bool IsPicoc { get; set; }
        public Guid? LinkedResearchQuestionId { get; set; }

        /// <summary>
        /// Danh sách fields trong section (recursive tree structure)
        /// </summary>
        public List<ExtractionFieldDto> Fields { get; set; } = new();

        /// <summary>
        /// Danh sách cột cho MatrixGrid section
        /// </summary>
        public List<ExtractionMatrixColumnDto> MatrixColumns { get; set; } = new();
    }

    public class ExtractionMatrixColumnDto
    {
        public Guid? ColumnId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
    }

    public class ExtractionFieldDto
    {
        public Guid? FieldId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? ParentFieldId { get; set; }

        [Required(ErrorMessage = "Name là bắt buộc")]
        [StringLength(500, ErrorMessage = "Name không được vượt quá 500 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Instruction { get; set; }

        /// <summary>
        /// Field Type: 0=Text, 1=Integer, 2=Decimal, 3=Boolean, 4=SingleSelect, 5=MultiSelect
        /// </summary>
        [Required(ErrorMessage = "FieldType là bắt buộc")]
        public int FieldType { get; set; }

        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }

        /// <summary>
        /// Options cho SingleSelect/MultiSelect
        /// </summary>
        public List<FieldOptionDto> Options { get; set; } = new();

        /// <summary>
        /// Sub-fields (nested structure)
        /// </summary>
        public List<ExtractionFieldDto> SubFields { get; set; } = new();
    }

    public class FieldOptionDto
    {
        /// <summary>
        /// OptionId - nullable, FE có thể send null hoặc invalid GUID
        /// </summary>
        public Guid? OptionId { get; set; }

        /// <summary>
        /// FieldId - nullable, sẽ được fill bởi service
        /// </summary>
        public Guid? FieldId { get; set; }

        [Required(ErrorMessage = "Value là bắt buộc")]
        [StringLength(500)]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Display order - default 0 if not provided
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }
}