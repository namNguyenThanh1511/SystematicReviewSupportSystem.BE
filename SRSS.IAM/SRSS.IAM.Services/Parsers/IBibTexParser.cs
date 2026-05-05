using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Parsers;

public interface IBibTexParser
{
    /// <summary>
    /// Parses a BibTeX-formatted stream and returns a list of paper DTOs.
    /// </summary>
    List<RisPaperDto> Parse(Stream stream);
}
