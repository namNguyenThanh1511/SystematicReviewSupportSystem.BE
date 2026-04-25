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
				Name = entity.Name
			};
		}

		public static SearchSource ToEntity(this SearchSourceDto dto)
		{
			return new SearchSource
			{
				Id = dto.SourceId ?? Guid.Empty,
				ProjectId = dto.ProjectId,
				MasterSourceId = dto.MasterSourceId,
				Name = dto.Name
			};
		}

		public static void UpdateEntity(this SearchSourceDto dto, SearchSource entity)
		{
			entity.ProjectId = dto.ProjectId;
			entity.MasterSourceId = dto.MasterSourceId;
			entity.Name = dto.Name;
		}

		// ==================== List Mapping ====================
		public static List<SearchSourceDto> ToDtoList(this IEnumerable<SearchSource> entities)
		{
			return entities.Select(e => e.ToDto()).ToList();
		}
	}
}