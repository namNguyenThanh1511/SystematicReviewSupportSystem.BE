using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.StudySelectionChecklist
{
    public class LiveReviewChecklistDto
    {
        public string Title { get; set; } = string.Empty;
        public List<LiveReviewParagraphDto> Paragraphs { get; set; } = new();
        public List<LiveReviewSectionDto> Sections { get; set; } = new();
    }

    public class LiveReviewParagraphDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class LiveReviewSectionDto
    {
        public string Title { get; set; } = string.Empty;
        public List<LiveReviewItemDto>? Items { get; set; }
    }

    public class LiveReviewItemDto
    {
        public string Text { get; set; } = string.Empty;
    }
}
