using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Repositories.Entities
{
    public class SearchStrategy : BaseEntity<Guid>
    {
        public Guid SearchSourceId { get; set; }
        public string Query { get; set; } = string.Empty;
        public string[] Fields { get; set; } = Array.Empty<string>();
        
        // Keywords breakdown
        public string[] PopulationKeywords { get; set; } = Array.Empty<string>();
        public string[] InterventionKeywords { get; set; } = Array.Empty<string>();
        public string[] ComparisonKeywords { get; set; } = Array.Empty<string>();
        public string[] OutcomeKeywords { get; set; } = Array.Empty<string>();
        public string[] ContextKeywords { get; set; } = Array.Empty<string>();

        public DateTimeOffset? DateSearched { get; set; }
        public string? Version { get; set; }
        public string? Notes { get; set; }
        public string? FiltersJson { get; set; } // Store as JSON string

        // Navigation property
        public SearchSource SearchSource { get; set; } = null!;
    }
}
