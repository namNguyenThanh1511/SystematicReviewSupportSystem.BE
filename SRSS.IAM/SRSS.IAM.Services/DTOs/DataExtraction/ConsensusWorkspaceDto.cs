using System.ComponentModel.DataAnnotations;

namespace SRSS.IAM.Services.DTOs.DataExtraction
{
    public class ConsensusWorkspaceDto
    {
        public Guid PaperId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid Reviewer1Id { get; set; }
        public Guid Reviewer2Id { get; set; }
        public List<ConsensusSectionDto> Sections { get; set; } = new();
    }

    public class ConsensusSectionDto
    {
        public Guid SectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SectionType { get; set; }
        public int OrderIndex { get; set; }
        public List<ConsensusFieldDto> Fields { get; set; } = new();
        public List<ExtractionMatrixColumnDto> MatrixColumns { get; set; } = new();
    }

    public class ConsensusFieldDto
    {
        public Guid FieldId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Instruction { get; set; }
        public int FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }
        
        public List<FieldOptionDto> Options { get; set; } = new();
        public List<ExtractedAnswerDto> Answers { get; set; } = new();
        public List<ConsensusFieldDto> SubFields { get; set; } = new();
    }

    public class ExtractedAnswerDto
    {
        public Guid? MatrixColumnId { get; set; }
        public int? MatrixRowIndex { get; set; }
        public AnswerDetailDto? Reviewer1Answer { get; set; }
        public AnswerDetailDto? Reviewer2Answer { get; set; }
    }

    public class AnswerDetailDto
    {
        public Guid? OptionId { get; set; }
        public string? StringValue { get; set; }
        public decimal? NumericValue { get; set; }
        public bool? BooleanValue { get; set; }
        public string? DisplayValue { get; set; }
    }

    public class SubmitConsensusRequestDto
    {
        public List<ExtractedValueDto> FinalValues { get; set; } = new();
    }
}
