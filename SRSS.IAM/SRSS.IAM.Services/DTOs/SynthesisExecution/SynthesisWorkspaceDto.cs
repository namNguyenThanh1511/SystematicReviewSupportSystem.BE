using SRSS.IAM.Repositories.Entities.Enums;
using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.SynthesisExecution
{
    public class SynthesisProcessDto
    {
        public Guid Id { get; set; }
        public Guid ReviewProcessId { get; set; }
        public SynthesisProcessStatus Status { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }

    public class SynthesisWorkspaceDto
    {
        public SynthesisProcessDto Process { get; set; } = null!;
        public int TotalExtractedPapers { get; set; }
        public List<SynthesisThemeDto> Themes { get; set; } = new();
        public List<ResearchQuestionFindingDto> Findings { get; set; } = new();
    }
}
