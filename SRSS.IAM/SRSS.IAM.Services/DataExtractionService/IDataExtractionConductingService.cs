using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.DataExtractionService
{
	public interface IDataExtractionConductingService
	{
		Task<ExtractionDashboardResponseDto> GetDashboardAsync(Guid extractionProcessId, ExtractionDashboardFilterDto filter);
		Task AssignReviewersAsync(Guid extractionProcessId, Guid paperId, AssignReviewersDto dto);
		Task<DataExtractionProcessResponse> StartAsync(Guid extractionProcessId);
		Task SubmitExtractionAsync(Guid extractionProcessId, Guid paperId, SubmitExtractionRequestDto request);
		Task<ConsensusWorkspaceDto> GetConsensusWorkspaceAsync(Guid extractionProcessId, Guid paperId);
		Task SubmitConsensusAsync(Guid extractionProcessId, Guid paperId, SubmitConsensusRequestDto request);
		Task<byte[]> ExportExtractedDataAsync(Guid extractionProcessId);
		Task<List<ExtractedValueDto>> AutoExtractWithAiAsync(Guid extractionProcessId, Guid paperId);
	}
}
