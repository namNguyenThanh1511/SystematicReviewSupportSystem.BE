using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.AiSetup
{
    public class AnalyzeTopicRequest
    {
        public string Topic { get; set; } = string.Empty;
    }

    public class AnalyzeTopicResponse
    {
        public string Objectives { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }

    public class GeneratePicocRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }

    public class GeneratePicocResponse
    {
        public string Population { get; set; } = string.Empty;
        public string Intervention { get; set; } = string.Empty;
        public string Comparator { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }

    public class GenerateRqsRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public GeneratePicocResponse Picoc { get; set; } = new();
    }

    public class GenerateRqsResponse
    {
        public List<string> SuggestedQuestions { get; set; } = new();
    }

    public class UpdateSetupRequest
    {
        public string ResearchTopic { get; set; } = string.Empty;
        public string ResearchObjective { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public GeneratePicocResponse Picoc { get; set; } = new();
        public List<ResearchQuestionInputDto> FinalResearchQuestions { get; set; } = new();
    }

    public class ResearchQuestionInputDto
    {
        public Guid? Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }

    public class ProjectSetupDetailsResponse
    {
        public string ResearchTopic { get; set; } = string.Empty;
        public string ResearchObjective { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public GeneratePicocResponse Picoc { get; set; } = new();
        public List<ResearchQuestionDetailDto> ResearchQuestions { get; set; } = new();
    }

    public class ResearchQuestionDetailDto
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }
}
