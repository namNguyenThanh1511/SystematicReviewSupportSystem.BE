using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.Mappers
{
	public static class DataExtractionMappingExtension
	{
		// ==================== DataExtractionStrategy ====================
		public static DataExtractionStrategyDto ToDto(this DataExtractionStrategy entity)
		{
			return new DataExtractionStrategyDto
			{
				ExtractionStrategyId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Description = entity.Description
			};
		}

		public static DataExtractionStrategy ToEntity(this DataExtractionStrategyDto dto)
		{
			return new DataExtractionStrategy
			{
				Id = dto.ExtractionStrategyId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this DataExtractionStrategyDto dto, DataExtractionStrategy entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Description = dto.Description;
		}

		// ==================== DataExtractionForm ====================
		public static DataExtractionFormDto ToDto(this DataExtractionForm entity)
		{
			return new DataExtractionFormDto
			{
				FormId = entity.Id,
				ExtractionStrategyId = entity.ExtractionStrategyId,
				Name = entity.Name
			};
		}

		public static DataExtractionForm ToEntity(this DataExtractionFormDto dto)
		{
			return new DataExtractionForm
			{
				Id = dto.FormId ?? Guid.Empty,
				ExtractionStrategyId = dto.ExtractionStrategyId,
				Name = dto.Name
			};
		}

		public static void UpdateEntity(this DataExtractionFormDto dto, DataExtractionForm entity)
		{
			entity.ExtractionStrategyId = dto.ExtractionStrategyId;
			entity.Name = dto.Name;
		}

		// ==================== DataItemDefinition ====================
		public static DataItemDefinitionDto ToDto(this DataItemDefinition entity)
		{
			return new DataItemDefinitionDto
			{
				DataItemId = entity.Id,
				FormId = entity.FormId,
				Name = entity.Name,
				DataType = entity.DataType,
				Description = entity.Description
			};
		}

		public static DataItemDefinition ToEntity(this DataItemDefinitionDto dto)
		{
			return new DataItemDefinition
			{
				Id = dto.DataItemId ?? Guid.Empty,
				FormId = dto.FormId,
				Name = dto.Name,
				DataType = dto.DataType,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this DataItemDefinitionDto dto, DataItemDefinition entity)
		{
			entity.FormId = dto.FormId;
			entity.Name = dto.Name;
			entity.DataType = dto.DataType;
			entity.Description = dto.Description;
		}
	}
}