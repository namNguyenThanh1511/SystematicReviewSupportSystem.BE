using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.Utils;

namespace SRSS.IAM.Services.Parsers;

/// <summary>
/// Wraps the static <see cref="BibTexParser"/> utility to implement <see cref="IBibTexParser"/>.
/// </summary>
public sealed class BibTexFileParser : IBibTexParser
{
    public List<RisPaperDto> Parse(Stream stream)
    {
        var papers = BibTexParser.Parse(stream);

        if (!papers.Any())
            throw new InvalidOperationException("No valid records found in the BibTeX file.");

        return papers;
    }
}
