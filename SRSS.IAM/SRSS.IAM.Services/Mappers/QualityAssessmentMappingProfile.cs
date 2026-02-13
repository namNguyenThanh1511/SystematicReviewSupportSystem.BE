using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.QualityAssessment;
using SRSS.IAM.Services.DTOs.SelectionCriteria;

namespace SRSS.IAM.Services.Mappers
{
	public static class QualityAssessmentMappingExtension
	{
		// ==================== QualityAssessmentStrategy ====================
		public static QualityAssessmentStrategyDto ToDto(this QualityAssessmentStrategy entity)
		{
			return new QualityAssessmentStrategyDto
			{
				QaStrategyId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Description = entity.Description
			};
		}

		public static QualityAssessmentStrategy ToEntity(this QualityAssessmentStrategyDto dto)
		{
			return new QualityAssessmentStrategy
			{
				Id = dto.QaStrategyId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this QualityAssessmentStrategyDto dto, QualityAssessmentStrategy entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Description = dto.Description;
		}

		// ==================== QualityChecklist ====================
		public static QualityChecklistDto ToDto(this QualityChecklist entity)
		{
			return new QualityChecklistDto
			{
				ChecklistId = entity.Id,
				QaStrategyId = entity.QaStrategyId,
				Name = entity.Name
			};
		}

		public static QualityChecklist ToEntity(this QualityChecklistDto dto)
		{
			return new QualityChecklist
			{
				Id = dto.ChecklistId ?? Guid.Empty,
				QaStrategyId = dto.QaStrategyId,
				Name = dto.Name
			};
		}

		public static void UpdateEntity(this QualityChecklistDto dto, QualityChecklist entity)
		{
			entity.QaStrategyId = dto.QaStrategyId;
			entity.Name = dto.Name;
		}

		// ==================== QualityCriterion ====================
		public static QualityCriterionDto ToDto(this QualityCriterion entity)
		{
			return new QualityCriterionDto
			{
				QualityCriterionId = entity.Id,
				ChecklistId = entity.ChecklistId,
				Question = entity.Question,
				Weight = entity.Weight
			};
		}

		public static QualityCriterion ToEntity(this QualityCriterionDto dto)
		{
			return new QualityCriterion
			{
				Id = dto.QualityCriterionId ?? Guid.Empty,
				ChecklistId = dto.ChecklistId,
				Question = dto.Question,
				Weight = dto.Weight
			};
		}

		public static void UpdateEntity(this QualityCriterionDto dto, QualityCriterion entity)
		{
			entity.ChecklistId = dto.ChecklistId;
			entity.Question = dto.Question;
			entity.Weight = dto.Weight;
		}

		// ==================== List Mapping ====================
		public static List<QualityAssessmentStrategyDto> ToDtoList(this IEnumerable<QualityAssessmentStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<QualityChecklistDto> ToDtoList(this IEnumerable<QualityChecklist> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<QualityCriterionDto> ToDtoList(this IEnumerable<QualityCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<StudySelectionCriteriaDto> ToDtoList(this IEnumerable<StudySelectionCriteria> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<InclusionCriterionDto> ToDtoList(this IEnumerable<InclusionCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<ExclusionCriterionDto> ToDtoList(this IEnumerable<ExclusionCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<StudySelectionProcedureDto> ToDtoList(this IEnumerable<StudySelectionProcedure> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}