using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.DTOs.SearchStrategy;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.DTOs.Synthesis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.DTOs.Protocol
{
	public class ProtocolDetailResponse
	{
		public Guid ProtocolId { get; set; }
		public Guid ProjectId { get; set; }
		public string ProtocolVersion { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public bool IsDeleted { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset? ApprovedAt { get; set; }
		public List<VersionHistoryDto> Versions { get; set; } = new();
		public StudyCharacteristicsDto? StudyCharacteristics { get; set; }

		// Related Protocol Details
		public List<SearchSourceDto> SearchSources { get; set; } = new();
		public List<StudySelectionCriteriaDto> SelectionCriterias { get; set; } = new();
		public List<StudySelectionProcedureDto> SelectionProcedures { get; set; } = new();
		public List<QualityAssessmentStrategyDto> QualityStrategies { get; set; } = new();
		public List<ExtractionTemplateDto> ExtractionTemplates { get; set; } = new();
		public List<DataSynthesisStrategyDto> SynthesisStrategies { get; set; } = new();
	}

	public class VersionHistoryDto
	{
		public Guid VersionId { get; set; }
		public string VersionNumber { get; set; } = string.Empty;
		public string? ChangeSummary { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
	}
}