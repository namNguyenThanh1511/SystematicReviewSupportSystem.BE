using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.StudySelection
{
    public class StuSeAIInput
    {
        public StuSePaperInput Paper { get; set; } = null!;
        public List<StuSeCriteriaGroupInput> CriteriaGroups { get; set; } = new();
    }

    public class StuSeCriteriaGroupInput
    {
        public string? Description { get; set; }
        public List<string> Inclusion { get; set; } = new();
        public List<string> Exclusion { get; set; } = new();
    }

    public class StuSePaperInput
    {
        public string Title { get; set; } = string.Empty;
        public string? Abstract { get; set; }
        public string? Keywords { get; set; }
        public int? PublicationYear { get; set; }
        public string? Language { get; set; }
    }
}
