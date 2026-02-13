using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DTOs.SearchStrategy;

namespace SRSS.IAM.Services.Mappers
{
	public static class SearchStrategyMappingExtension
	{
		// ==================== SearchStrategy ====================
		public static SearchStrategyDto ToDto(this SearchStrategy entity)
		{
			return new SearchStrategyDto
			{
				StrategyId = entity.Id,
				ProtocolId = entity.ProtocolId,
				Description = entity.Description
			};
		}

		public static SearchStrategy ToEntity(this SearchStrategyDto dto)
		{
			return new SearchStrategy
			{
				Id = dto.StrategyId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				Description = dto.Description
			};
		}

		public static void UpdateEntity(this SearchStrategyDto dto, SearchStrategy entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.Description = dto.Description;
		}

		// ==================== SearchString ====================
		public static SearchStringDto ToDto(this SearchString entity)
		{
			return new SearchStringDto
			{
				SearchStringId = entity.Id,
				StrategyId = entity.StrategyId,
				Expression = entity.Expression
			};
		}

		public static SearchString ToEntity(this SearchStringDto dto)
		{
			return new SearchString
			{
				Id = dto.SearchStringId ?? Guid.Empty,
				StrategyId = dto.StrategyId,
				Expression = dto.Expression
			};
		}

		public static void UpdateEntity(this SearchStringDto dto, SearchString entity)
		{
			entity.StrategyId = dto.StrategyId;
			entity.Expression = dto.Expression;
		}

		// ==================== SearchTerm ====================
		public static SearchTermDto ToDto(this SearchTerm entity)
		{
			return new SearchTermDto
			{
				TermId = entity.Id,
				Keyword = entity.Keyword,
				Source = entity.Source
			};
		}

		public static SearchTerm ToEntity(this SearchTermDto dto)
		{
			return new SearchTerm
			{
				Id = dto.TermId ?? Guid.Empty,
				Keyword = dto.Keyword,
				Source = dto.Source
			};
		}

		public static void UpdateEntity(this SearchTermDto dto, SearchTerm entity)
		{
			entity.Keyword = dto.Keyword;
			entity.Source = dto.Source;
		}

		// ==================== SearchStringTerm ====================
		public static SearchStringTermDto ToDto(this SearchStringTerm entity)
		{
			return new SearchStringTermDto
			{
				SearchStringId = entity.SearchStringId,
				TermId = entity.TermId
			};
		}

		public static SearchStringTerm ToEntity(this SearchStringTermDto dto)
		{
			return new SearchStringTerm
			{
				SearchStringId = dto.SearchStringId,
				TermId = dto.TermId
			};
		}

		// ==================== SearchSource ====================
		public static SearchSourceDto ToDto(this SearchSource entity)
		{
			return new SearchSourceDto
			{
				SourceId = entity.Id,
				ProtocolId = entity.ProtocolId,
				SourceType = entity.SourceType,
				Name = entity.Name
			};
		}

		public static SearchSource ToEntity(this SearchSourceDto dto)
		{
			return new SearchSource
			{
				Id = dto.SourceId ?? Guid.Empty,
				ProtocolId = dto.ProtocolId,
				SourceType = dto.SourceType,
				Name = dto.Name
			};
		}

		public static void UpdateEntity(this SearchSourceDto dto, SearchSource entity)
		{
			entity.ProtocolId = dto.ProtocolId;
			entity.SourceType = dto.SourceType;
			entity.Name = dto.Name;
		}

		// ==================== List Mapping ====================
		public static List<DataExtractionStrategyDto> ToDtoList(this IEnumerable<DataExtractionStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<DataExtractionFormDto> ToDtoList(this IEnumerable<DataExtractionForm> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<DataItemDefinitionDto> ToDtoList(this IEnumerable<DataItemDefinition> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<SearchStrategyDto> ToDtoList(this IEnumerable<SearchStrategy> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<SearchStringDto> ToDtoList(this IEnumerable<SearchString> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<SearchTermDto> ToDtoList(this IEnumerable<SearchTerm> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<SearchStringTermDto> ToDtoList(this IEnumerable<SearchStringTerm> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}

		public static List<SearchSourceDto> ToDtoList(this IEnumerable<SearchSource> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}