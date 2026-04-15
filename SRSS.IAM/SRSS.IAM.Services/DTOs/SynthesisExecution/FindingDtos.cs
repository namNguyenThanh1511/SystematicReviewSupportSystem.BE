using System;
using System.ComponentModel.DataAnnotations;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.SynthesisExecution
{
    public class ResearchQuestionFindingDto
    {
        public Guid Id { get; set; }
        public Guid ResearchQuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public FindingStatus Status { get; set; }
        public Guid AuthorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }

    public class SaveFindingRequest
    {
        [Required]
        public string AnswerText { get; set; } = string.Empty;
        public FindingStatus Status { get; set; }
    }
}
