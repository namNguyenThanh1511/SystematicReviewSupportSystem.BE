using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.DTOs.OpenAlex;

namespace SRSS.IAM.Services.OpenAlex
{
    public interface IOpenAlexService
    {
        Task<WorkDetailDto> GetWorkAsync(string workId, CancellationToken ct);

        Task<ReferenceResultDto> GetReferencesAsync(string workId, CancellationToken ct);

        Task<CitationResultDto> GetCitationsAsync(
            string workId,
            int pageSize = 100,
            int? maxResults = null,
            CancellationToken ct = default);
    }
}
