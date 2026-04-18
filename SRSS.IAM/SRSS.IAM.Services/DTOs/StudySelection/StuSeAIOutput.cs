using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.StudySelection
{
    public class StuSeAIOutput
    {
        public List<ResearchQuestionResult> ResearchQuestionResults { get; set; } = new();
        public List<CriteriaGroupResult> CriteriaGroupResults { get; set; } = new();
        public int InclusionMatches { get; set; }
        public int ExclusionMatches { get; set; }
        public List<string> ExclusionHighlights { get; set; } = new();
        public double RelevanceScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
    }

    public class ResearchQuestionResult
    {
        public string Question { get; set; } = string.Empty;
        public string Match { get; set; } = "Unknown";
        public PicocMatchingResult? PicocMatching { get; set; }
    }

    public class CriteriaGroupResult
    {
        public string? Description { get; set; }
        public List<InclusionCriteriaResult> InclusionResults { get; set; } = new();
        public List<ExclusionCriteriaResult> ExclusionResults { get; set; } = new();
    }

    public class MatchingValue
    {
        public string? Value { get; set; }
        public string Match { get; set; } = "Unknown";
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
