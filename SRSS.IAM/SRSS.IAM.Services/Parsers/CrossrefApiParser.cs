using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.Crossref;
using SRSS.IAM.Services.DTOs.Crossref;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.Parsers;

/// <summary>
/// Queries the Crossref search API with a <see cref="CrossrefQueryParameters"/> object
/// and returns all matched works as a uniform <see cref="RisPaperDto"/> list.
/// </summary>
public sealed class CrossrefApiParser : IApiParser<CrossrefQueryParameters>
{
    private readonly ICrossrefService _crossrefService;
    private readonly ILogger<CrossrefApiParser> _logger;

    public CrossrefApiParser(ICrossrefService crossrefService, ILogger<CrossrefApiParser> logger)
    {
        _crossrefService = crossrefService;
        _logger = logger;
    }

    public async Task<List<RisPaperDto>> ParseAsync(CrossrefQueryParameters query, CancellationToken ct = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        _logger.LogInformation("Querying Crossref API with parameters: {@Query}", query);

        var messageList = await _crossrefService.GetWorksAsync(query, ct);
        var items = messageList.Items;

        if (!items.Any())
            throw new InvalidOperationException("Crossref API returned no results for the given query parameters.");

        var papers = items.Select(CrossrefMapper.ToRisPaper).ToList();

        _logger.LogInformation("Crossref API returned {Count} record(s).", papers.Count);

        return papers;
    }
}
