using SRSS.IAM.Services.DTOs.DataExtraction;

namespace SRSS.IAM.Services.DataExtractionService
{
	public interface IDataExtractionConductingService
	{
		Task<ExtractionDashboardResponseDto> GetDashboardAsync(Guid extractionProcessId, ExtractionDashboardFilterDto filter);
		Task AssignReviewersAsync(Guid extractionProcessId, Guid paperId, AssignReviewersDto dto);
		Task<DataExtractionProcessResponse> StartAsync(Guid extractionProcessId);
	}
}
