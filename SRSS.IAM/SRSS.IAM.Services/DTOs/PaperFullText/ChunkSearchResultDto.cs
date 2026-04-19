using System;

namespace SRSS.IAM.Services.DTOs.PaperFullText
{
    public class ChunkSearchResultDto
    {
        public Guid ChunkId { get; set; }
        public int Order { get; set; }
        public string SectionTitle { get; set; } = string.Empty;
        public string? SectionType { get; set; }
        public string Text { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
    }
}
