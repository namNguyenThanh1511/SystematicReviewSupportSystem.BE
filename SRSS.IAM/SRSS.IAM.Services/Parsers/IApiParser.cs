using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Parsers;

public interface IApiParser<TQuery>
{
    /// <summary>
    /// Queries an external API with <paramref name="query"/> and maps all results
    /// to a uniform list of paper DTOs.
    /// </summary>
    Task<List<RisPaperDto>> ParseAsync(TQuery query, CancellationToken ct = default);
}
