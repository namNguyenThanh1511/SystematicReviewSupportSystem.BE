using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.IdentificationService
{
    public interface IIdentificationService
    {
        Task<IdentificationProcessResponse> CreateIdentificationProcessAsync(CreateIdentificationProcessRequest request, CancellationToken cancellationToken = default);
        
        Task<SearchExecutionResponse> CreateSearchExecutionAsync(CreateSearchExecutionRequest request, CancellationToken cancellationToken = default);
        Task<SearchExecutionResponse?> GetSearchExecutionByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<SearchExecutionResponse>> GetSearchExecutionsByIdentificationProcessIdAsync(Guid identificationProcessId, CancellationToken cancellationToken = default);
        Task<SearchExecutionResponse> UpdateSearchExecutionAsync(UpdateSearchExecutionRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteSearchExecutionAsync(Guid id, CancellationToken cancellationToken = default);
        
        Task<ImportPaperResponse> ImportPaperAsync(ImportPaperRequest request, CancellationToken cancellationToken = default);
        Task<RisImportResultDto> ImportRisFileAsync(
            Stream fileStream, 
            string fileName,
            string? source,
            string? importedBy,
            Guid? searchExecutionId,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default);
    }
}


