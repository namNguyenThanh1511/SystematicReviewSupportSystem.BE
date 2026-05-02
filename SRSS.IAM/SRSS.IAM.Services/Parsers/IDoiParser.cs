using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Parsers;

public interface IDoiParser
{
    /// <summary>
    /// Resolves a single DOI and returns a list containing the matched paper DTO.
    /// </summary>
    Task<List<RisPaperDto>> ParseAsync(string doi, CancellationToken ct = default);
}
