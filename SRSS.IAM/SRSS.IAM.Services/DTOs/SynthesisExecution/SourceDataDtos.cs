using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.SynthesisExecution
{
    public class SourceDataValueDto
    {
        public Guid ExtractedDataValueId { get; set; }
        public Guid PaperId { get; set; }
        public string PaperTitle { get; set; } = string.Empty;
        public string? StringValue { get; set; }
        public decimal? NumericValue { get; set; }
        public bool? BooleanValue { get; set; }
        public Guid? OptionId { get; set; }
        public string DisplayValue { get; set; } = string.Empty;
    }

    public class SourceDataGroupDto
    {
        public Guid FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public List<SourceDataValueDto> Values { get; set; } = new List<SourceDataValueDto>();
    }
}
