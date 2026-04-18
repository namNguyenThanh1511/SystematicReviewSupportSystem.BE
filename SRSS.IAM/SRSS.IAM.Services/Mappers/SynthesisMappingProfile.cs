using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.Synthesis;

namespace SRSS.IAM.Services.Mappers
{
	public static class SynthesisMappingExtension
	{
		// ==================== DataSynthesisStrategy ====================
		public static DataSynthesisStrategyDto ToDto(this DataSynthesisStrategy entity)
		{
			return new DataSynthesisStrategyDto
			{
				SynthesisStrategyId = entity.Id,
				ProtocolId = entity.ProtocolId,
				SynthesisType = entity.SynthesisType,
				Description = entity.Description,
				TargetResearchQuestionIds = entity.TargetResearchQuestionIds ?? new List<Guid>(),
				DataGroupingPlan = entity.DataGroupingPlan,
				SensitivityAnalysisPlan = entity.SensitivityAnalysisPlan
			};
		}

		public static DataSynthesisStrategy ToEntity(this DataSynthesisStrategyDto dto)
		{
			return new DataSynthesisStrategy
			{
				Id = dto.SynthesisStrategyId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				SynthesisType = dto.SynthesisType,
				Description = dto.Description,
				TargetResearchQuestionIds = dto.TargetResearchQuestionIds ?? new List<Guid>(),
				DataGroupingPlan = dto.DataGroupingPlan,
				SensitivityAnalysisPlan = dto.SensitivityAnalysisPlan
			};
		}

		public static void UpdateEntity(this DataSynthesisStrategyDto dto, DataSynthesisStrategy entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.SynthesisType = dto.SynthesisType;
			entity.Description = dto.Description;
			entity.TargetResearchQuestionIds = dto.TargetResearchQuestionIds ?? new List<Guid>();
			entity.DataGroupingPlan = dto.DataGroupingPlan;
			entity.SensitivityAnalysisPlan = dto.SensitivityAnalysisPlan;
		}



		// ==================== ProjectTimetable ====================
		public static ProjectTimetableDto ToDto(this ProjectTimetable entity)
		{
			return new ProjectTimetableDto
			{
				TimetableId = entity.Id,
				ProjectId = entity.ProjectId,
				Milestone = entity.Milestone,
				PlannedDate = entity.PlannedDate
			};
		}

		public static ProjectTimetable ToEntity(this ProjectTimetableDto dto)
		{
			return new ProjectTimetable
			{
				Id = dto.TimetableId ?? Guid.Empty,
				ProjectId = dto.ProjectId,
				Milestone = dto.Milestone,
				PlannedDate = dto.PlannedDate
			};
		}

		public static void UpdateEntity(this ProjectTimetableDto dto, ProjectTimetable entity)
		{
			entity.ProjectId = dto.ProjectId;
			entity.Milestone = dto.Milestone;
			entity.PlannedDate = dto.PlannedDate;
		}

		// ==================== LIST MAPPING METHODS ====================

		
		public static List<DataSynthesisStrategyDto> ToDtoList(this IEnumerable<DataSynthesisStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}



	
		public static List<ProjectTimetableDto> ToDtoList(this IEnumerable<ProjectTimetable> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}