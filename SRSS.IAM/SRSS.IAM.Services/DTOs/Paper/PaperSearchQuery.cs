using SRSS.IAM.Services.DTOs.Common;

namespace SRSS.IAM.Services.DTOs.Paper
{
    /// <summary>
    /// Query object for searching papers within a project with filtering capabilities
    /// </summary>
    public class PaperSearchQuery : PaginationRequest
    {
        /// <summary>
        /// Text search query to filter papers by title, abstract, authors, keywords, etc.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Search strategy ID to filter papers imported via a specific search strategy
        /// </summary>
        public Guid? SearchStrategyId { get; set; }

        /// <summary>
        /// Search source ID to filter papers from a specific database source (e.g., PubMed, Scopus, etc.)
        /// </summary>
        public Guid? SearchSourceId { get; set; }

        /// <summary>
        /// Publication year to filter papers by year of publication
        /// </summary>
        public int? Year { get; set; }
    }
}
