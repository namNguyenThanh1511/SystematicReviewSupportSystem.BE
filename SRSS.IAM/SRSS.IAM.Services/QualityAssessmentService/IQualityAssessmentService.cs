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

		// Quality Assessment Processes
		Task<QualityAssessmentProcessResponse> GetProcessByReviewProcessIdAsync(Guid reviewProcessId);
        Task<QualityAssessmentProcessResponse> CreateProcessAsync(CreateQualityAssessmentProcessDto dto);
        Task<QualityAssessmentProcessResponse> UpdateProcessAsync(Guid id, UpdateQualityAssessmentProcessDto dto);

        // Assignments
        Task AssignPapersToReviewersAsync(CreateQualityAssessmentAssignmentDto dto);
        Task<List<MyAssignedPaperDto>> GetMyAssignedPapersAsync(Guid userId, Guid reviewProcessId);

        // Decisions
        Task CreateDecisionAsync(Guid userId, CreateQualityAssessmentDecisionDto dto);
        Task CreateDecisionsForPaperAsync(Guid userId, Guid paperId, List<CreateQualityAssessmentDecisionItemDto> dtos);
        Task UpdateDecisionAsync(Guid userId, Guid paperId, Guid criterionId, UpdateQualityAssessmentDecisionDto dto);
        Task UpdateDecisionsForPaperAsync(Guid userId, Guid paperId, List<UpdateQualityAssessmentDecisionItemDto> dtos);
        Task<List<QualityAssessmentDecisionDto>> GetDecisionsByPaperIdAsync(Guid paperId);

        // Resolutions
        Task<QualityAssessmentResolutionResponse> CreateResolutionAsync(CreateQualityAssessmentResolutionDto dto);
        Task<QualityAssessmentResolutionResponse> UpdateResolutionAsync(Guid id, UpdateQualityAssessmentResolutionDto dto);
        Task<QualityAssessmentResolutionResponse> GetResolutionByPaperIdAsync(Guid paperId);
	}
}