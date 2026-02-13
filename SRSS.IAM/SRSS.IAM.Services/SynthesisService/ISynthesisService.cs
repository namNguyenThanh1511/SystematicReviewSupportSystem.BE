using SRSS.IAM.Services.DTOs.Synthesis;

namespace SRSS.IAM.Services.SynthesisService
{
	public interface ISynthesisService
	{
		// Data Synthesis Strategies
		Task<DataSynthesisStrategyDto> UpsertSynthesisStrategyAsync(DataSynthesisStrategyDto dto);
		Task<List<DataSynthesisStrategyDto>> GetSynthesisStrategiesByProtocolIdAsync(Guid protocolId);
		Task DeleteSynthesisStrategyAsync(Guid strategyId);

		// Dissemination Strategies
		Task<DisseminationStrategyDto> UpsertDisseminationStrategyAsync(DisseminationStrategyDto dto);
		Task<List<DisseminationStrategyDto>> GetDisseminationStrategiesByProtocolIdAsync(Guid protocolId);
		Task DeleteDisseminationStrategyAsync(Guid strategyId);

		// Project Timetable
		Task<List<ProjectTimetableDto>> BulkUpsertTimetableAsync(List<ProjectTimetableDto> dtos);
		Task<List<ProjectTimetableDto>> GetTimetableByProtocolIdAsync(Guid protocolId);
		Task DeleteTimetableEntryAsync(Guid timetableId);
	}
}