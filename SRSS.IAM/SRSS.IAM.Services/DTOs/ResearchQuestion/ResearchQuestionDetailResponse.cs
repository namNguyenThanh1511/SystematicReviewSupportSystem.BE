using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.DTOs.ResearchQuestion
{
	public class ResearchQuestionDetailResponse
	{
		public Guid ResearchQuestionId { get; set; }
		public Guid ProjectId { get; set; }
		public string QuestionType { get; set; } = string.Empty;
		public string QuestionText { get; set; } = string.Empty;
		public string? Rationale { get; set; }
		public List<PicocElementDto> PicocElements { get; set; } = new();
		public DateTimeOffset CreatedAt { get; set; }
	}

	public class PicocElementDto
	{
		public Guid PicocId { get; set; }
		public string ElementType { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
        /// <summary>
        /// General object to hold Population/Intervention/Comparison/Outcome/Context
        /// </summary>
        public object? SpecificDetail { get; set; }
	}
}