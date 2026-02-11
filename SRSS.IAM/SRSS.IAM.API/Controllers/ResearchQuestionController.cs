using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.ResearchQuestion;
using SRSS.IAM.Services.ResearchQuestionService;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/research-questions")]
	[Authorize]
	public class ResearchQuestionController : BaseController
	{
		private readonly IResearchQuestionService _researchQuestionService;

		public ResearchQuestionController(IResearchQuestionService researchQuestionService)
		{
			_researchQuestionService = researchQuestionService;
		}

		/// <summary>
		/// Tạo research question kèm PICOC elements trong một transaction
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<ApiResponse<ResearchQuestionDetailResponse>>> CreateResearchQuestion(
			[FromBody] CreateResearchQuestionRequest request)
		{
			var result = await _researchQuestionService.CreateResearchQuestionAsync(request);
			return Created(result, "Tạo research question thành công");
		}

		/// <summary>
		/// Lấy danh sách research questions theo project
		/// </summary>
		[HttpGet("project/{projectId}")]
		public async Task<ActionResult<ApiResponse<List<ResearchQuestionDetailResponse>>>> GetResearchQuestionsByProject(
			Guid projectId)
		{
			var result = await _researchQuestionService.GetResearchQuestionsByProjectIdAsync(projectId);
			return Ok(result, "Lấy danh sách research questions thành công");
		}
	}
}