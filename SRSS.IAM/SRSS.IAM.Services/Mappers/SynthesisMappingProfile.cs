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





		// ==================== LIST MAPPING METHODS ====================

		
		public static List<DataSynthesisStrategyDto> ToDtoList(this IEnumerable<DataSynthesisStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}




	}
}