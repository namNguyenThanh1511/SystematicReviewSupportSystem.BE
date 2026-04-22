using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.StudySelection
{
    public class StuSeAIInput
    {
        public StuSePaperInput Paper { get; set; } = null!;
        public List<StuSeRQInput> ResearchQuestions { get; set; } = new();
        public List<StuSeCriteriaGroupInput> CriteriaGroups { get; set; } = new();
    }

    public class StuSeRQInput
    {
        public string QuestionText { get; set; } = string.Empty;
        public StuSePicocInput? PICOC { get; set; }
    }

    public class StuSeCriteriaGroupInput
    {
        public string? Description { get; set; }
        public List<string> InclusionRules { get; set; } = new();
        public List<string> ExclusionRules { get; set; } = new();
    }

    public class StuSePaperInput
    {
        public string Title { get; set; } = string.Empty;
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        public int? PublicationYear { get; set; }
        public string? Language { get; set; }
    }


    public class StuSePicocInput
    {
        public string? Population { get; set; }
        public string? Intervention { get; set; }
        public string? Comparison { get; set; }
        public string? Outcome { get; set; }
        public string? Context { get; set; }
    }
}
