using SRSS.IAM.Services.DTOs.QualityAssessment;

namespace SRSS.IAM.Services.QualityAssessmentService
{
	public interface IQualityAssessmentService
	{
		// Quality Assessment Strategies
		Task<QualityAssessmentStrategyDto> UpsertStrategyAsync(QualityAssessmentStrategyDto dto);
		Task<List<QualityAssessmentStrategyDto>> GetStrategiesByProtocolIdAsync(Guid protocolId);
		Task<List<QualityAssessmentStrategyDto>> GetStrategiesByProcessIdAsync(Guid processId);
		Task DeleteStrategyAsync(Guid strategyId);

		// Quality Checklists
		Task<List<QualityAssessmentChecklistDto>> BulkUpsertChecklistsAsync(List<QualityAssessmentChecklistDto> dtos);
		Task<List<QualityAssessmentChecklistDto>> GetChecklistsByStrategyIdAsync(Guid strategyId);

		// Quality Criteria
		Task<List<QualityAssessmentCriterionDto>> BulkUpsertCriteriaAsync(List<QualityAssessmentCriterionDto> dtos);
		Task<List<QualityAssessmentCriterionDto>> GetCriteriaByChecklistIdAsync(Guid checklistId);

		// Quality Assessment Processes
		Task<QualityAssessmentProcessResponse> GetProcessByReviewProcessIdAsync(Guid reviewProcessId);
        Task<QualityAssessmentProcessResponse> CreateProcessAsync(CreateQualityAssessmentProcessDto dto);
		Task<QualityAssessmentProcessResponse> StartProcessAsync(Guid qaId);
		Task<QualityAssessmentProcessResponse> CompleteProcessAsync(Guid qaId);

		// Papers
		Task<List<QualityAssessmentPaperDto>> GetAllPapersAsync(Guid qaId);

		// Assignments
		Task AssignPapersToReviewersAsync(CreateQualityAssessmentAssignmentDto dto);
        Task<List<AssignedPaperDto>> GetMyAssignedPapersAsync(Guid reviewProcessId);

        // Decisions
        Task CreateDecisionAsync(CreateQualityAssessmentDecisionDto dto);
        Task CreateDecisionsForPaperAsync(Guid paperId, List<CreateQualityAssessmentDecisionItemDto> dtos);
        Task UpdateDecisionAsync(Guid id, UpdateQualityAssessmentDecisionDto dto);
        Task UpdateDecisionsBatchAsync(List<UpdateQualityAssessmentDecisionItemDto> dtos);
        Task<List<QualityAssessmentDecisionDto>> GetDecisionsByPaperIdAsync(Guid paperId);

        // Resolutions
        Task<QualityAssessmentResolutionResponse> CreateResolutionAsync(CreateQualityAssessmentResolutionDto dto);
        Task<QualityAssessmentResolutionResponse> UpdateResolutionAsync(Guid id, UpdateQualityAssessmentResolutionDto dto);
        Task<QualityAssessmentResolutionResponse> GetResolutionByPaperIdAsync(Guid paperId);
	}
}