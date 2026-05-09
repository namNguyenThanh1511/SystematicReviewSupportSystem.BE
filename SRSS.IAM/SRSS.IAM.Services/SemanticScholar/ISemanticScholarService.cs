using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.SemanticScholar;

namespace SRSS.IAM.Services.SemanticScholar;

public interface ISemanticScholarService
{
    Task<PaginatedResponse<PaperSearchResultDto>> SearchPapersAsync(SemanticScholarSearchRequest request, CancellationToken ct = default);
}
