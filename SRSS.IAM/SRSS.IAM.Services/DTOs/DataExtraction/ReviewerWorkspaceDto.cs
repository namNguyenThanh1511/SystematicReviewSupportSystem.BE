using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ReviewerWorkspaceDto
    {
        public Guid PaperId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid ReviewerId { get; set; }
        public List<ReviewerSectionDto> Sections { get; set; } = new();
    }

    public class ReviewerSectionDto
    {
        public Guid SectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SectionType { get; set; }
        public int OrderIndex { get; set; }
        public List<ReviewerFieldDto> Fields { get; set; } = new();
        public List<ExtractionMatrixColumnDto> MatrixColumns { get; set; } = new();
    }

    public class ReviewerFieldDto
    {
        public Guid FieldId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Instruction { get; set; }
        public int FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }

        public List<FieldOptionDto> Options { get; set; } = new();
        public List<ReviewerExtractedAnswerDto> Answers { get; set; } = new();
        public List<ReviewerFieldDto> SubFields { get; set; } = new();
    }

    public class ReviewerExtractedAnswerDto
    {
        public Guid? MatrixColumnId { get; set; }
        public int? MatrixRowIndex { get; set; }
        public AnswerDetailDto? Answer { get; set; }
    }
}
