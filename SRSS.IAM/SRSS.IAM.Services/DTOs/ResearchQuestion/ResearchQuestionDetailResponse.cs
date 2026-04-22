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
		public string? QuestionType { get; set; }
		public string QuestionText { get; set; } = string.Empty;
		public string? Rationale { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
	}
}