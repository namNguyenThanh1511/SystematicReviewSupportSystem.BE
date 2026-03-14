using SRSS.IAM.Services.DTOs.ResearchQuestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.ResearchQuestionService
{
	public interface IResearchQuestionService
	{
		Task<ResearchQuestionDetailResponse> CreateResearchQuestionAsync(CreateResearchQuestionRequest request);
		Task<List<ResearchQuestionDetailResponse>> GetResearchQuestionsByProjectIdAsync(Guid projectId);
	}
}
