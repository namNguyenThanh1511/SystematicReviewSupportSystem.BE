using SRSS.IAM.Services.DTOs.Synthesis;

namespace SRSS.IAM.Services.SynthesisService
{
	public interface ISynthesisService
	{
		// Data Synthesis Strategies
		Task<DataSynthesisStrategyDto> UpsertSynthesisStrategyAsync(DataSynthesisStrategyDto dto);
		Task<List<DataSynthesisStrategyDto>> GetSynthesisStrategiesByProtocolIdAsync(Guid protocolId);
		Task DeleteSynthesisStrategyAsync(Guid strategyId);




	}
}