using System.Text.Json;
using SRSS.IAM.Repositories.Entities;
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
				ProjectId = entity.ProjectId,
				MasterSourceId = entity.MasterSourceId,
				Name = entity.Name,
				Url = entity.MasterSource?.BaseUrl,
				Strategies = entity.Strategies?.Select(s => s.ToDto()).ToList() ?? new()
			};
		}

		public static SearchSource ToEntity(this SearchSourceDto dto)
		{
			return new SearchSource
			{
				Id = dto.SourceId ?? Guid.Empty,
				ProjectId = dto.ProjectId,
				MasterSourceId = dto.MasterSourceId,
				Name = dto.Name,
				Strategies = dto.Strategies?.Select(s => s.ToEntity()).ToList() ?? new List<SearchStrategy>()
			};
		}

		public static void UpdateEntity(this SearchSourceDto dto, SearchSource entity)
		{
			entity.ProjectId = dto.ProjectId;
			entity.MasterSourceId = dto.MasterSourceId;
			entity.Name = dto.Name;
		}

		// ==================== SearchStrategy ====================
		public static SearchStrategyDto ToDto(this SearchStrategy entity)
		{
			return new SearchStrategyDto
			{
				Id = entity.Id,
				Query = entity.Query,
				Fields = entity.Fields,
				PopulationKeywords = entity.PopulationKeywords,
				InterventionKeywords = entity.InterventionKeywords,
				ComparisonKeywords = entity.ComparisonKeywords,
				OutcomeKeywords = entity.OutcomeKeywords,
				ContextKeywords = entity.ContextKeywords,
				DateSearched = entity.DateSearched,
				Version = entity.Version,
				Notes = entity.Notes,
				Filters = string.IsNullOrEmpty(entity.FiltersJson) 
					? new SearchFiltersDto() 
					: JsonSerializer.Deserialize<SearchFiltersDto>(entity.FiltersJson) ?? new()
			};
		}

		public static SearchStrategy ToEntity(this SearchStrategyDto dto)
		{
			return new SearchStrategy
			{
				Id = dto.Id ?? Guid.Empty,
				Query = dto.Query,
				Fields = dto.Fields,
				PopulationKeywords = dto.PopulationKeywords,
				InterventionKeywords = dto.InterventionKeywords,
				ComparisonKeywords = dto.ComparisonKeywords,
				OutcomeKeywords = dto.OutcomeKeywords,
				ContextKeywords = dto.ContextKeywords,
				DateSearched = dto.DateSearched?.ToUniversalTime(),
				Version = dto.Version,
				Notes = dto.Notes,
				FiltersJson = JsonSerializer.Serialize(dto.Filters)
			};
		}

		public static void UpdateEntity(this SearchStrategyDto dto, SearchStrategy entity)
		{
			entity.Query = dto.Query;
			entity.Fields = dto.Fields;
			entity.PopulationKeywords = dto.PopulationKeywords;
			entity.InterventionKeywords = dto.InterventionKeywords;
			entity.ComparisonKeywords = dto.ComparisonKeywords;
			entity.OutcomeKeywords = dto.OutcomeKeywords;
			entity.ContextKeywords = dto.ContextKeywords;
			entity.DateSearched = dto.DateSearched?.ToUniversalTime();
			entity.Version = dto.Version;
			entity.Notes = dto.Notes;
			entity.FiltersJson = JsonSerializer.Serialize(dto.Filters);
		}

		// ==================== List Mapping ====================
		public static List<SearchSourceDto> ToDtoList(this IEnumerable<SearchSource> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}