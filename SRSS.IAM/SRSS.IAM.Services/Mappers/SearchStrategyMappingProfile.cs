using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DTOs.SearchStrategy;

namespace SRSS.IAM.Services.Mappers
{
	public static class SearchStrategyMappingExtension
	{

		// ==================== SearchSource ====================
		public static SearchSourceDto ToDto(this SearchSource entity)
		{
			return new SearchSourceDto
			{
				SourceId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Name = entity.Name
			};
		}

		public static SearchSource ToEntity(this SearchSourceDto dto)
		{
			return new SearchSource
			{
				Id = dto.SourceId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Name = dto.Name
			};
		}

		public static void UpdateEntity(this SearchSourceDto dto, SearchSource entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Name = dto.Name;
		}

		// ==================== List Mapping ====================
		//public static List<DataExtractionStrategyDto> ToDtoList(this IEnumerable<DataExtractionStrategy> entities)
		//{
		//	return entities.Select(e => e.ToDto()).ToList();
		//}

		//public static List<DataExtractionFormDto> ToDtoList(this IEnumerable<DataExtractionForm> entities)
		//{
		//	return entities.Select(e => e.ToDto()).ToList();
		//}

		//public static List<DataItemDefinitionDto> ToDtoList(this IEnumerable<DataItemDefinition> entities)
		//{
		//	return entities.Select(e => e.ToDto()).ToList();
		//}

		public static List<SearchSourceDto> ToDtoList(this IEnumerable<SearchSource> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}