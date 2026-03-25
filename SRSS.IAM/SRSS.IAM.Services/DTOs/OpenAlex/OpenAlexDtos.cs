using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SRSS.IAM.Services.DTOs.OpenAlex
{
    public class WorkDetailDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("doi")]
        public string? Doi { get; set; }

        [JsonPropertyName("publication_year")]
        public int PublicationYear { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("cited_by_count")]
        public int CitedByCount { get; set; }

        [JsonPropertyName("referenced_works_count")]
        public int ReferencedWorksCount { get; set; }

        [JsonPropertyName("cited_by_percentile_year")]
        public CitedByPercentileDto? CitedByPercentileYear { get; set; }
    }

    public class CitedByPercentileDto
    {
        [JsonPropertyName("min")]
        public double Min { get; set; }

        [JsonPropertyName("max")]
        public double Max { get; set; }
    }


    public class ReferenceResultDto
    {
        public string WorkId { get; set; } = string.Empty;

        [JsonPropertyName("referenced_works")]
        public List<string> ReferencedWorks { get; set; } = new();

        [JsonPropertyName("referenced_works_count")]
        public int ReferencedWorksCount { get; set; }
    }

    public class WorkSummaryDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("publication_year")]
        public int PublicationYear { get; set; }
    }

    public class CitationResultDto
    {
        public int TotalCount { get; set; }
        public List<WorkSummaryDto> Works { get; set; } = new();
    }

    internal class OpenAlexResponse<T>
    {
        [JsonPropertyName("meta")]
        public OpenAlexMeta Meta { get; set; } = new();

        [JsonPropertyName("results")]
        public List<T> Results { get; set; } = new();
    }

    internal class OpenAlexMeta
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("next_cursor")]
        public string? NextCursor { get; set; }
    }
}
