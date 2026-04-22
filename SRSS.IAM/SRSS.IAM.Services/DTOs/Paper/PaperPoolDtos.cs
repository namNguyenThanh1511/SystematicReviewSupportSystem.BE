using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.DTOs.Paper
{
    public class PaperPoolQueryRequest : PaginationRequest
    {
        public string? SearchText { get; set; }
        public string? Keyword { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public string? SearchSourceId { get; set; } = "all";
        public string? ImportBatchId { get; set; } = "all";
        public string DoiState { get; set; } = "all";
        public string FullTextState { get; set; } = "all";
        public bool OnlyUnused { get; set; }
        public bool RecentlyImported { get; set; }
    }

    public class PaperPoolFilterMetadataResponse
    {
        public List<FilterOptionResponse> SearchSources { get; set; } = new();
        public List<FilterOptionResponse> ImportBatches { get; set; } = new();
    }

    public class FilterOptionResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class FilterStateDto
    {
        public string? Keyword { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public string SearchSourceId { get; set; } = "all";
        public string ImportBatchId { get; set; } = "all";
        public string DoiState { get; set; } = "all";
        public string FullTextState { get; set; } = "all";
        public bool OnlyUnused { get; set; }
        public bool RecentlyImported { get; set; }
    }

    public class FilterSettingRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? SearchText { get; set; }
        public FilterStateDto Filters { get; set; } = new();
    }

    public class FilterSettingResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SearchText { get; set; }
        public FilterStateDto Filters { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
    }
}
