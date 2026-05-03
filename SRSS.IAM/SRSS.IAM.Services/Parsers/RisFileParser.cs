using SRSS.IAM.Services.DTOs.Identification;
using SRSS.IAM.Services.Utils;

namespace SRSS.IAM.Services.Parsers;

/// <summary>
/// Wraps the static <see cref="RisParser"/> utility to implement <see cref="IRisParser"/>.
/// </summary>
public sealed class RisFileParser : IRisParser
{
    public List<RisPaperDto> Parse(Stream stream)
    {
        var papers = RisParser.Parse(stream);

        if (!papers.Any())
            throw new InvalidOperationException("No valid records found in the RIS file.");

        return papers;
    }
}
