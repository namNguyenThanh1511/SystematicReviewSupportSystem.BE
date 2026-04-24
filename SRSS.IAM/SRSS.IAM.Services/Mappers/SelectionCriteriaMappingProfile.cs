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
				StudySelectionProcessId = entity.StudySelectionProcessId,
				Description = entity.Description,
				InclusionCriteria = entity.InclusionCriteria?.Select(i => i.ToDto()).ToList() ?? new(),
				ExclusionCriteria = entity.ExclusionCriteria?.Select(e => e.ToDto()).ToList() ?? new()
			};
		}

		public static StudySelectionCriteria ToEntity(this StudySelectionCriteriaDto dto)
		{
			return new StudySelectionCriteria
			{
				Id = dto.CriteriaId ?? Guid.Empty,
				StudySelectionProcessId = dto.StudySelectionProcessId,
				Description = dto.Description ?? string.Empty
			};
		}

		public static void UpdateEntity(this StudySelectionCriteriaDto dto, StudySelectionCriteria entity)
		{
			entity.StudySelectionProcessId = dto.StudySelectionProcessId;
			entity.Description = dto.Description ?? string.Empty;
		}

		public static List<StudySelectionCriteriaDto> ToDtoList(this IEnumerable<StudySelectionCriteria> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
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

		public static List<InclusionCriterionDto> ToDtoList(this IEnumerable<InclusionCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
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

		public static List<ExclusionCriterionDto> ToDtoList(this IEnumerable<ExclusionCriterion> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

	}
}