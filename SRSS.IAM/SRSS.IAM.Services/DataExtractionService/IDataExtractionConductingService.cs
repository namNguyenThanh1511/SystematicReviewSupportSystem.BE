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
		Task<ExtractionPreviewDto> GetPivotedExtractionDataAsync(Guid extractionProcessId);
		Task ReopenExtractionAsync(Guid extractionProcessId, Guid paperId, ReopenExtractionRequestDto request);
		Task<List<ExtractedValueDto>> AutoExtractWithAiAsync(Guid extractionProcessId, Guid paperId);
		Task<ExtractedValueDto?> AskAiSingleFieldAsync(Guid extractionProcessId, AskAiFieldRequestDto request, CancellationToken cancellationToken = default);
		Task DirectExtractByLeaderAsync(Guid extractionProcessId, Guid paperId, SubmitExtractionRequestDto payload, CancellationToken cancellationToken);
		Task<ExtractionWorkloadSummaryDto> GetWorkloadSummaryAsync(Guid extractionProcessId, CancellationToken cancellationToken);
	}
}
