using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ExtractionGridCellDto
    {
        public Guid PaperId { get; set; }
        public Guid FieldId { get; set; }
        public Guid? MatrixColumnId { get; set; }
        public int? MatrixRowIndex { get; set; }
        public string? Value { get; set; }
        public string? FieldType { get; set; }

        /// <summary>
        /// True when the consensus value for this cell is "Not Reported".
        /// The Value will be "NR" while this flag drives UI rendering.
        /// </summary>
        public bool IsNotReported { get; set; } = false;
    }

    public class ExtractionGridRowDto
    {
        public string RowId { get; set; } = string.Empty;
        public string PaperTitle { get; set; } = string.Empty;
        public string Citation { get; set; } = string.Empty;
        public Dictionary<string, ExtractionGridCellDto> Cells { get; set; } = new();
    }

    public class GridFieldOptionDto
    {
        public Guid OptionId { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class ExtractionGridColumnMetaDto
    {
        public Guid FieldId { get; set; }
        public string HeaderName { get; set; } = string.Empty; // Keep this as the unique key for Dictionary lookup
        public string SectionName { get; set; } = string.Empty;
        public string DisplayFieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public List<GridFieldOptionDto> Options { get; set; } = new();
    }

    public class ExtractionEditableGridDto
    {
        public List<ExtractionGridColumnMetaDto> Columns { get; set; } = new();
        public List<ExtractionGridRowDto> Rows { get; set; } = new();
    }
}
