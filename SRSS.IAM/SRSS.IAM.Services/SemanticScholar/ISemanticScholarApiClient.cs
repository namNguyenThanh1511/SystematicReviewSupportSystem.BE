using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.SemanticScholar;

namespace SRSS.IAM.Services.SemanticScholar;

/// <summary>
/// Internal client for direct Semantic Scholar API interaction.
/// Only the BackgroundWorker should use this.
/// </summary>
public interface ISemanticScholarApiClient
{
    Task<PaginatedResponse<PaperSearchResultDto>> ExecuteSearchAsync(SemanticScholarSearchRequest request, CancellationToken ct);
}
