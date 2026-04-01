using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.GrobidClient.DTOs
{
    public class GrobidReferenceDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? PublishedYear { get; set; }
        public string? DOI { get; set; }
        public string? RawReference { get; set; }
        public ReferenceType ReferenceType { get; set; } = ReferenceType.Unknown;
    }
}
