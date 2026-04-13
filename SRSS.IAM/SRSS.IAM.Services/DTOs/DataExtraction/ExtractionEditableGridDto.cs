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
        public string HeaderName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public List<GridFieldOptionDto> Options { get; set; } = new();
    }

    public class ExtractionEditableGridDto
    {
        public List<ExtractionGridColumnMetaDto> Columns { get; set; } = new();
        public List<ExtractionGridRowDto> Rows { get; set; } = new();
    }
}
