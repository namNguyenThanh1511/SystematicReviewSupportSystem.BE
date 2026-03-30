using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.StudySelection
{
    public class StuSeAIOutput
    {
        public CriteriaMatchingResult CriteriaMatching { get; set; } = new();
        public PicocMatchingResult PicocMatching { get; set; } = new();
        public List<ResearchQuestionMatch> ResearchQuestionMatching { get; set; } = new();
        public List<InclusionCriteriaResult> InclusionCriteriaResults { get; set; } = new();
        public List<ExclusionCriteriaResult> ExclusionCriteriaResults { get; set; } = new();
        public int InclusionMatches { get; set; }
        public int ExclusionMatches { get; set; }
        public List<string> ExclusionHighlights { get; set; } = new();
        public double RelevanceScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
    }

    public class MatchingValue
    {
        public string? Value { get; set; }
        public string Match { get; set; } = "Unknown";
    }

    public class CriteriaMatchingResult
    {
        public MatchingValue Language { get; set; } = new();
        public MatchingValue Domain { get; set; } = new();
        public MatchingValue StudyType { get; set; } = new();
        public MatchingValue TimeRange { get; set; } = new();
    }

    public class PicocMatchingResult
    {
        public MatchingValue Population { get; set; } = new();
        public MatchingValue Intervention { get; set; } = new();
        public MatchingValue Comparison { get; set; } = new();
        public MatchingValue Outcome { get; set; } = new();
        public MatchingValue Context { get; set; } = new();
    }

    public class ResearchQuestionMatch
    {
        public string Question { get; set; } = string.Empty;
        public string Match { get; set; } = "Unknown";
    }

    public class InclusionCriteriaResult
    {
        public string Rule { get; set; } = string.Empty;
        public string Match { get; set; } = "Unknown";
    }

    public class ExclusionCriteriaResult
    {
        public string Rule { get; set; } = string.Empty;
        public string Match { get; set; } = "Unknown";
        public string? Highlight { get; set; } = string.Empty;
    }
}
