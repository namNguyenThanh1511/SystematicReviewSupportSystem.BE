using SRSS.IAM.Services.DTOs.SearchStrategy;

namespace SRSS.IAM.Services.SearchStrategyService
{
	public interface ISearchStrategyService
	{
		// Search Source
		Task<List<SearchSourceDto>> BulkUpsertSearchSourcesAsync(List<SearchSourceDto> dtos);
		Task<List<SearchSourceDto>> GetSearchSourcesByProjectIdAsync(Guid projectId);
	}
}