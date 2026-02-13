using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.DataExtractionService
{
	public interface IDataExtractionService
	{
		// Extraction Strategies
		Task<DataExtractionStrategyDto> UpsertStrategyAsync(DataExtractionStrategyDto dto);
		Task<List<DataExtractionStrategyDto>> GetStrategiesByProtocolIdAsync(Guid protocolId);
		Task DeleteStrategyAsync(Guid strategyId);

		// Extraction Forms
		Task<List<DataExtractionFormDto>> BulkUpsertFormsAsync(List<DataExtractionFormDto> dtos);
		Task<List<DataExtractionFormDto>> GetFormsByStrategyIdAsync(Guid strategyId);

		// Data Items
		Task<List<DataItemDefinitionDto>> BulkUpsertDataItemsAsync(List<DataItemDefinitionDto> dtos);
		Task<List<DataItemDefinitionDto>> GetDataItemsByFormIdAsync(Guid formId);
	}
}