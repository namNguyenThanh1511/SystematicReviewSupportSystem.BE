using Microsoft.Extensions.Logging;
using SRSS.IAM.Services.Crossref;
using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.Parsers;

/// <summary>
/// Resolves a single DOI via Crossref and returns a unified <see cref="RisPaperDto"/> list.
/// The underlying <see cref="ICrossrefService"/> already handles caching for DOI lookups.
/// </summary>
public sealed class CrossrefDoiParser : IDoiParser
{
    private readonly ICrossrefService _crossrefService;
    private readonly ILogger<CrossrefDoiParser> _logger;

    public CrossrefDoiParser(ICrossrefService crossrefService, ILogger<CrossrefDoiParser> logger)
    {
        _crossrefService = crossrefService;
        _logger = logger;
    }

    public async Task<List<RisPaperDto>> ParseAsync(string doi, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(doi))
            throw new ArgumentException("DOI must not be null or empty.", nameof(doi));

        _logger.LogInformation("Resolving DOI via Crossref: {Doi}", doi);

        var work = await _crossrefService.GetWorkByDoiAsync(doi, ct);

        var paper = CrossrefMapper.ToRisPaper(work);

        _logger.LogInformation("DOI resolved successfully: {Doi} → \"{Title}\"", doi, paper.Title);

        return [paper];
    }
}
