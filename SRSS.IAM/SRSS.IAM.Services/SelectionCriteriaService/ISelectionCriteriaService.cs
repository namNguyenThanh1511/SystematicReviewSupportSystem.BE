using SRSS.IAM.Services.DTOs.SelectionCriteria;

namespace SRSS.IAM.Services.SelectionCriteriaService
{
	public interface ISelectionCriteriaService
	{
		// Study Selection Criteria
		Task<StudySelectionCriteriaDto> UpsertCriteriaAsync(StudySelectionCriteriaDto dto);
		Task<List<StudySelectionCriteriaDto>> GetAllByProtocolIdAsync(Guid protocolId);
		Task DeleteCriteriaAsync(Guid criteriaId);

		// Inclusion Criteria
		Task<List<InclusionCriterionDto>> BulkUpsertInclusionCriteriaAsync(List<InclusionCriterionDto> dtos);
		Task<List<InclusionCriterionDto>> GetInclusionByCriteriaIdAsync(Guid criteriaId);

		// Exclusion Criteria
		Task<List<ExclusionCriterionDto>> BulkUpsertExclusionCriteriaAsync(List<ExclusionCriterionDto> dtos);
		Task<List<ExclusionCriterionDto>> GetExclusionByCriteriaIdAsync(Guid criteriaId);

		// Selection Procedures
		Task<StudySelectionProcedureDto> UpsertProcedureAsync(StudySelectionProcedureDto dto);
		Task<List<StudySelectionProcedureDto>> GetProceduresByProtocolIdAsync(Guid protocolId);
		Task DeleteProcedureAsync(Guid procedureId);
	}
}