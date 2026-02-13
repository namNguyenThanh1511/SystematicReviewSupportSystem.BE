using SRSS.IAM.Services.DTOs.SearchStrategy;

namespace SRSS.IAM.Services.SearchStrategyService
{
	public interface ISearchStrategyService
	{
		// Search Strategy
		Task<SearchStrategyDto> UpsertAsync(SearchStrategyDto dto);
		Task<List<SearchStrategyDto>> GetAllByProtocolIdAsync(Guid protocolId);
		Task DeleteAsync(Guid strategyId);

		// Search String
		Task<List<SearchStringDto>> BulkUpsertSearchStringsAsync(List<SearchStringDto> dtos);
		Task<List<SearchStringDto>> GetSearchStringsByStrategyIdAsync(Guid strategyId);

		// Search Term
		Task<List<SearchTermDto>> BulkUpsertSearchTermsAsync(List<SearchTermDto> dtos);
		Task<List<SearchTermDto>> GetSearchTermsBySearchStringIdAsync(Guid searchStringId);

		// Search String Term (Junction)
		Task BulkUpsertSearchStringTermsAsync(List<SearchStringTermDto> dtos);
		Task<List<SearchStringTermDto>> GetSearchStringTermsBySearchStringIdAsync(Guid searchStringId);

		// Search Source
		Task<List<SearchSourceDto>> BulkUpsertSearchSourcesAsync(List<SearchSourceDto> dtos);
		Task<List<SearchSourceDto>> GetSearchSourcesByProtocolIdAsync(Guid protocolId);
	}
}