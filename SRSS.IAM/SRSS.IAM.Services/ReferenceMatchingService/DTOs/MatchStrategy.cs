using System;

namespace SRSS.IAM.Services.ReferenceMatchingService.DTOs
{
    public enum MatchStrategy
    {
        None = 0,
        DOI = 1,
        TitleExact = 2,
        TitleFuzzy = 3,
        AuthorYear = 4,
        Semantic = 5
    }
}
