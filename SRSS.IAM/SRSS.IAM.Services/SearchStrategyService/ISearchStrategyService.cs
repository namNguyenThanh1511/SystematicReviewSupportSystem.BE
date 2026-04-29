using SRSS.IAM.Services.DTOs.SearchStrategy;

namespace SRSS.IAM.Services.SearchStrategyService
{
	public interface ISearchStrategyService
	{
		// Search Source
		Task<List<SearchSourceDto>> BulkUpsertSearchSourcesAsync(List<SearchSourceDto> dtos);
		Task<SearchSourceDto> AddSearchSourceAsync(SearchSourceDto dto);
		Task<SearchSourceDto> UpdateSearchStrategiesAsync(Guid sourceId, List<SearchStrategyDto> strategies);
		Task<List<SearchSourceDto>> GetSearchSourcesByProjectIdAsync(Guid projectId);
	}
}