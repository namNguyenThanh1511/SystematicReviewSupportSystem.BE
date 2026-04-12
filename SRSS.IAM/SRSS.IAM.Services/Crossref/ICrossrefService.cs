using System.Threading;
using System.Threading.Tasks;
using SRSS.IAM.Services.DTOs.Crossref;

namespace SRSS.IAM.Services.Crossref;

public interface ICrossrefService
{
    Task<CrossrefMessageList<CrossrefWorkDto>> GetWorksAsync(CrossrefQueryParameters parameters, CancellationToken ct = default);
    Task<CrossrefWorkDto> GetWorkByDoiAsync(string doi, CancellationToken ct = default);
    Task<CrossrefAgencyDto> GetAgencyByDoiAsync(string doi, CancellationToken ct = default);
}
