using SRSS.IAM.Services.DTOs.Identification;

namespace SRSS.IAM.Services.Parsers;

public interface IRisParser
{
    /// <summary>
    /// Parses a RIS-formatted stream and returns a list of paper DTOs.
    /// </summary>
    List<RisPaperDto> Parse(Stream stream);
}
