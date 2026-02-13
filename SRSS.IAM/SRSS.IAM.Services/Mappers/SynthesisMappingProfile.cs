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
				Description = entity.Description
			};
		}

		public static DataSynthesisStrategy ToEntity(this DataSynthesisStrategyDto dto)
		{
			return new DataSynthesisStrategy
			{
				Id = dto.SynthesisStrategyId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				SynthesisType = dto.SynthesisType,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this DataSynthesisStrategyDto dto, DataSynthesisStrategy entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.SynthesisType = dto.SynthesisType;
			entity.Description = dto.Description;
		}

		// ==================== DisseminationStrategy ====================
		public static DisseminationStrategyDto ToDto(this DisseminationStrategy entity)
		{
			return new DisseminationStrategyDto
			{
				DisseminationId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Channel = entity.Channel,
				Description = entity.Description
			};
		}

		public static DisseminationStrategy ToEntity(this DisseminationStrategyDto dto)
		{
			return new DisseminationStrategy
			{
				Id = dto.DisseminationId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Channel = dto.Channel,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this DisseminationStrategyDto dto, DisseminationStrategy entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Channel = dto.Channel;
			entity.Description = dto.Description;
		}

		// ==================== ProjectTimetable ====================
		public static ProjectTimetableDto ToDto(this ProjectTimetable entity)
		{
			return new ProjectTimetableDto
			{
				TimetableId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Milestone = entity.Milestone,
				PlannedDate = entity.PlannedDate
			};
		}

		public static ProjectTimetable ToEntity(this ProjectTimetableDto dto)
		{
			return new ProjectTimetable
			{
				Id = dto.TimetableId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Milestone = dto.Milestone,
				PlannedDate = dto.PlannedDate
			};
		}

		public static void UpdateEntity(this ProjectTimetableDto dto, ProjectTimetable entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Milestone = dto.Milestone;
			entity.PlannedDate = dto.PlannedDate;
		}

		// ==================== LIST MAPPING METHODS ====================

		
		public static List<DataSynthesisStrategyDto> ToDtoList(this IEnumerable<DataSynthesisStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		
		public static List<DisseminationStrategyDto> ToDtoList(this IEnumerable<DisseminationStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

	
		public static List<ProjectTimetableDto> ToDtoList(this IEnumerable<ProjectTimetable> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}