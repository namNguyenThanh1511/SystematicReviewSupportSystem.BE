namespace SRSS.IAM.Services.DTOs.PaperFullText
{
    public class ParsedPaperFullTextDto
    {
        public List<ParsedSectionDto> Sections { get; set; } = new();
    }

    public class ParsedSectionDto
    {
        public int Order { get; set; }
        public string SectionTitle { get; set; } = string.Empty;
        public string? SectionType { get; set; }
        public string? Coordinates { get; set; }
        public List<ParsedParagraphDto> Paragraphs { get; set; } = new();
    }

    public class ParsedParagraphDto
    {
        public int Order { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Coordinates { get; set; }
    }
}
