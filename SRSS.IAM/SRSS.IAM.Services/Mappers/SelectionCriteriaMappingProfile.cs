using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.SelectionCriteria;

namespace SRSS.IAM.Services.Mappers
{
	public static class SelectionCriteriaMappingExtension
	{
		// ==================== StudySelectionCriteria ====================
		public static StudySelectionCriteriaDto ToDto(this StudySelectionCriteria entity)
		{
			return new StudySelectionCriteriaDto
			{
				CriteriaId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Description = entity.Description
			};
		}

		public static StudySelectionCriteria ToEntity(this StudySelectionCriteriaDto dto)
		{
			return new StudySelectionCriteria
			{
				Id = dto.CriteriaId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this StudySelectionCriteriaDto dto, StudySelectionCriteria entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Description = dto.Description;
		}

		// ==================== InclusionCriterion ====================
		public static InclusionCriterionDto ToDto(this InclusionCriterion entity)
		{
			return new InclusionCriterionDto
			{
				InclusionId = entity.Id,
				CriteriaId = entity.CriteriaId,
				Rule = entity.Rule
			};
		}

		public static InclusionCriterion ToEntity(this InclusionCriterionDto dto)
		{
			return new InclusionCriterion
			{
				Id = dto.InclusionId ?? Guid.Empty,
				CriteriaId = dto.CriteriaId,
				Rule = dto.Rule
			};
		}

		public static void UpdateEntity(this InclusionCriterionDto dto, InclusionCriterion entity)
		{
			entity.CriteriaId = dto.CriteriaId;
			entity.Rule = dto.Rule;
		}

		// ==================== ExclusionCriterion ====================
		public static ExclusionCriterionDto ToDto(this ExclusionCriterion entity)
		{
			return new ExclusionCriterionDto
			{
				ExclusionId = entity.Id,
				CriteriaId = entity.CriteriaId,
				Rule = entity.Rule
			};
		}

		public static ExclusionCriterion ToEntity(this ExclusionCriterionDto dto)
		{
			return new ExclusionCriterion
			{
				Id = dto.ExclusionId ?? Guid.Empty,
				CriteriaId = dto.CriteriaId,
				Rule = dto.Rule
			};
		}

		public static void UpdateEntity(this ExclusionCriterionDto dto, ExclusionCriterion entity)
		{
			entity.CriteriaId = dto.CriteriaId;
			entity.Rule = dto.Rule;
		}

		// ==================== StudySelectionProcedure ====================
		public static StudySelectionProcedureDto ToDto(this StudySelectionProcedure entity)
		{
			return new StudySelectionProcedureDto
			{
				ProcedureId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Steps = entity.Steps
			};
		}

		public static StudySelectionProcedure ToEntity(this StudySelectionProcedureDto dto)
		{
			return new StudySelectionProcedure
			{
				Id = dto.ProcedureId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Steps = dto.Steps
			};
		}

		public static void UpdateEntity(this StudySelectionProcedureDto dto, StudySelectionProcedure entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Steps = dto.Steps;
		}
	}
}