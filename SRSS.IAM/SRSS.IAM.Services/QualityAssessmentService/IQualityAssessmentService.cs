using SRSS.IAM.Services.DTOs.QualityAssessment;

namespace SRSS.IAM.Services.QualityAssessmentService
{
	public interface IQualityAssessmentService
	{
		// Quality Assessment Strategies
		Task<QualityAssessmentStrategyDto> UpsertStrategyAsync(QualityAssessmentStrategyDto dto);
		Task<List<QualityAssessmentStrategyDto>> GetStrategiesByProtocolIdAsync(Guid protocolId);
		Task DeleteStrategyAsync(Guid strategyId);

		// Quality Checklists
		Task<List<QualityChecklistDto>> BulkUpsertChecklistsAsync(List<QualityChecklistDto> dtos);
		Task<List<QualityChecklistDto>> GetChecklistsByStrategyIdAsync(Guid strategyId);

		// Quality Criteria
		Task<List<QualityCriterionDto>> BulkUpsertCriteriaAsync(List<QualityCriterionDto> dtos);
		Task<List<QualityCriterionDto>> GetCriteriaByChecklistIdAsync(Guid checklistId);
	}
}