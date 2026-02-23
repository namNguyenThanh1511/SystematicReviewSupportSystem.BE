using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.CoreGovernService;
using SRSS.IAM.Services.DTOs.CoreGovern;
using SRSS.IAM.Services.DTOs.ResearchQuestion;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/core-govern")]
	public class CoreGovernController : BaseController
	{
		private readonly ICoreGovernService _coreGovernService;

		public CoreGovernController(ICoreGovernService coreGovernService)
		{
			_coreGovernService = coreGovernService;
		}

		// ══════════════════════════ ReviewNeed ══════════════════════════════

		/// <summary>
		/// Tạo mới một review need cho dự án
		/// </summary>
		[HttpPost("review-needs")]
		public async Task<ActionResult<ApiResponse<ReviewNeedResponse>>> CreateReviewNeed(
			[FromBody] CreateReviewNeedRequest request)
		{
			var result = await _coreGovernService.CreateReviewNeedAsync(request);
			return Created(result, "Tạo review need thành công.");
		}

		/// <summary>
		/// Cập nhật review need
		/// </summary>
		[HttpPut("review-needs/{id}")]
		public async Task<ActionResult<ApiResponse<ReviewNeedResponse>>> UpdateReviewNeed(
			Guid id, [FromBody] UpdateReviewNeedRequest request)
		{
			if (id != request.Id)
				throw new ArgumentException("Route ID không khớp với body ID.");

			var result = await _coreGovernService.UpdateReviewNeedAsync(request);
			return Ok(result, "Cập nhật review need thành công.");
		}

		/// <summary>
		/// Xóa review need
		/// </summary>
		[HttpDelete("review-needs/{id}")]
		public async Task<ActionResult<ApiResponse>> DeleteReviewNeed(Guid id)
		{
			await _coreGovernService.DeleteReviewNeedAsync(id);
			return Ok("Xóa review need thành công.");
		}

		/// <summary>
		/// Lấy review need theo ID
		/// </summary>
		[HttpGet("review-needs/{id}")]
		public async Task<ActionResult<ApiResponse<ReviewNeedResponse>>> GetReviewNeedById(Guid id)
		{
			var result = await _coreGovernService.GetReviewNeedByIdAsync(id);
			return Ok(result, "Lấy review need thành công.");
		}

		/// <summary>
		/// Lấy danh sách review needs theo dự án
		/// </summary>
		[HttpGet("review-needs/project/{projectId}")]
		public async Task<ActionResult<ApiResponse<IEnumerable<ReviewNeedResponse>>>> GetReviewNeedsByProject(Guid projectId)
		{
			var result = await _coreGovernService.GetReviewNeedsByProjectIdAsync(projectId);
			return Ok(result, "Lấy danh sách review needs thành công.");
		}

		// ══════════════════════ CommissioningDocument ════════════════════════

		/// <summary>
		/// Tạo mới commissioning document cho dự án (mỗi dự án chỉ có một)
		/// </summary>
		[HttpPost("commissioning-documents")]
		public async Task<ActionResult<ApiResponse<CommissioningDocumentResponse>>> CreateCommissioningDocument(
			[FromBody] CreateCommissioningDocumentRequest request)
		{
			var result = await _coreGovernService.CreateCommissioningDocumentAsync(request);
			return Created(result, "Tạo commissioning document thành công.");
		}

		/// <summary>
		/// Cập nhật commissioning document
		/// </summary>
		[HttpPut("commissioning-documents/{id}")]
		public async Task<ActionResult<ApiResponse<CommissioningDocumentResponse>>> UpdateCommissioningDocument(
			Guid id, [FromBody] UpdateCommissioningDocumentRequest request)
		{
			if (id != request.Id)
				throw new ArgumentException("Route ID không khớp với body ID.");

			var result = await _coreGovernService.UpdateCommissioningDocumentAsync(request);
			return Ok(result, "Cập nhật commissioning document thành công.");
		}

		/// <summary>
		/// Xóa commissioning document
		/// </summary>
		[HttpDelete("commissioning-documents/{id}")]
		public async Task<ActionResult<ApiResponse>> DeleteCommissioningDocument(Guid id)
		{
			await _coreGovernService.DeleteCommissioningDocumentAsync(id);
			return Ok("Xóa commissioning document thành công.");
		}

		/// <summary>
		/// Lấy commissioning document theo ID
		/// </summary>
		[HttpGet("commissioning-documents/{id}")]
		public async Task<ActionResult<ApiResponse<CommissioningDocumentResponse>>> GetCommissioningDocumentById(Guid id)
		{
			var result = await _coreGovernService.GetCommissioningDocumentByIdAsync(id);
			return Ok(result, "Lấy commissioning document thành công.");
		}

		/// <summary>
		/// Lấy commissioning document theo dự án
		/// </summary>
		[HttpGet("commissioning-documents/project/{projectId}")]
		public async Task<ActionResult<ApiResponse<CommissioningDocumentResponse?>>> GetCommissioningDocumentByProject(Guid projectId)
		{
			var result = await _coreGovernService.GetCommissioningDocumentByProjectIdAsync(projectId);
			return Ok(result, "Lấy commissioning document thành công.");
		}

		// ══════════════════════════ ReviewObjective ══════════════════════════

		/// <summary>
		/// Tạo mới review objective cho dự án
		/// </summary>
		[HttpPost("review-objectives")]
		public async Task<ActionResult<ApiResponse<ReviewObjectiveResponse>>> CreateReviewObjective(
			[FromBody] CreateReviewObjectiveRequest request)
		{
			var result = await _coreGovernService.CreateReviewObjectiveAsync(request);
			return Created(result, "Tạo review objective thành công.");
		}

		/// <summary>
		/// Cập nhật review objective
		/// </summary>
		[HttpPut("review-objectives/{id}")]
		public async Task<ActionResult<ApiResponse<ReviewObjectiveResponse>>> UpdateReviewObjective(
			Guid id, [FromBody] UpdateReviewObjectiveRequest request)
		{
			if (id != request.Id)
				throw new ArgumentException("Route ID không khớp với body ID.");

			var result = await _coreGovernService.UpdateReviewObjectiveAsync(request);
			return Ok(result, "Cập nhật review objective thành công.");
		}

		/// <summary>
		/// Xóa review objective
		/// </summary>
		[HttpDelete("review-objectives/{id}")]
		public async Task<ActionResult<ApiResponse>> DeleteReviewObjective(Guid id)
		{
			await _coreGovernService.DeleteReviewObjectiveAsync(id);
			return Ok("Xóa review objective thành công.");
		}

		/// <summary>
		/// Lấy review objective theo ID
		/// </summary>
		[HttpGet("review-objectives/{id}")]
		public async Task<ActionResult<ApiResponse<ReviewObjectiveResponse>>> GetReviewObjectiveById(Guid id)
		{
			var result = await _coreGovernService.GetReviewObjectiveByIdAsync(id);
			return Ok(result, "Lấy review objective thành công.");
		}

		/// <summary>
		/// Lấy danh sách review objectives theo dự án
		/// </summary>
		[HttpGet("review-objectives/project/{projectId}")]
		public async Task<ActionResult<ApiResponse<IEnumerable<ReviewObjectiveResponse>>>> GetReviewObjectivesByProject(Guid projectId)
		{
			var result = await _coreGovernService.GetReviewObjectivesByProjectIdAsync(projectId);
			return Ok(result, "Lấy danh sách review objectives thành công.");
		}

		// ══════════════════════════ QuestionType ════════════════════════════

		/// <summary>
		/// Tạo mới question type
		/// </summary>
		[HttpPost("question-types")]
		public async Task<ActionResult<ApiResponse<QuestionTypeResponse>>> CreateQuestionType(
			[FromBody] CreateQuestionTypeRequest request)
		{
			var result = await _coreGovernService.CreateQuestionTypeAsync(request);
			return Created(result, "Tạo question type thành công.");
		}

		/// <summary>
		/// Cập nhật question type
		/// </summary>
		[HttpPut("question-types/{id}")]
		public async Task<ActionResult<ApiResponse<QuestionTypeResponse>>> UpdateQuestionType(
			Guid id, [FromBody] UpdateQuestionTypeRequest request)
		{
			if (id != request.Id)
				throw new ArgumentException("Route ID không khớp với body ID.");

			var result = await _coreGovernService.UpdateQuestionTypeAsync(request);
			return Ok(result, "Cập nhật question type thành công.");
		}

		/// <summary>
		/// Xóa question type
		/// </summary>
		[HttpDelete("question-types/{id}")]
		public async Task<ActionResult<ApiResponse>> DeleteQuestionType(Guid id)
		{
			await _coreGovernService.DeleteQuestionTypeAsync(id);
			return Ok("Xóa question type thành công.");
		}

		/// <summary>
		/// Lấy question type theo ID
		/// </summary>
		[HttpGet("question-types/{id}")]
		public async Task<ActionResult<ApiResponse<QuestionTypeResponse>>> GetQuestionTypeById(Guid id)
		{
			var result = await _coreGovernService.GetQuestionTypeByIdAsync(id);
			return Ok(result, "Lấy question type thành công.");
		}

		/// <summary>
		/// Lấy tất cả question types
		/// </summary>
		[HttpGet("question-types")]
		public async Task<ActionResult<ApiResponse<IEnumerable<QuestionTypeResponse>>>> GetAllQuestionTypes()
		{
			var result = await _coreGovernService.GetAllQuestionTypesAsync();
			return Ok(result, "Lấy danh sách question types thành công.");
		}

		// ══════════════════════════ ResearchQuestion ═════════════════════════

		/// <summary>
		/// Lấy research question theo ID kèm QuestionType và toàn bộ PICOC elements
		/// </summary>
		[HttpGet("research-questions/{id}")]
		public async Task<ActionResult<ApiResponse<ResearchQuestionDetailResponse>>> GetResearchQuestionById(Guid id)
		{
			var result = await _coreGovernService.GetResearchQuestionByIdAsync(id);
			return Ok(result, "Lấy research question thành công.");
		}

		/// <summary>
		/// Lấy toàn bộ research questions của một project kèm QuestionType và PICOC elements
		/// </summary>
		[HttpGet("research-questions/project/{projectId}")]
		public async Task<ActionResult<ApiResponse<IEnumerable<ResearchQuestionDetailResponse>>>> GetResearchQuestionsByProject(Guid projectId)
		{
			var result = await _coreGovernService.GetResearchQuestionsByProjectIdAsync(projectId);
			return Ok(result, "Lấy danh sách research questions thành công.");
		}

		/// <summary>
		/// Cập nhật nội dung research question (không bao gồm PICOC)
		/// </summary>
		[HttpPut("research-questions/{id}")]
		public async Task<ActionResult<ApiResponse<ResearchQuestionDetailResponse>>> UpdateResearchQuestion(
			Guid id, [FromBody] UpdateResearchQuestionRequest request)
		{
			if (id != request.Id)
				throw new ArgumentException("Route ID không khớp với body ID.");

			var result = await _coreGovernService.UpdateResearchQuestionAsync(request);
			return Ok(result, "Cập nhật research question thành công.");
		}

		/// <summary>
		/// Xóa research question kèm toàn bộ PICOC elements
		/// </summary>
		[HttpDelete("research-questions/{id}")]
		public async Task<ActionResult<ApiResponse>> DeleteResearchQuestion(Guid id)
		{
			await _coreGovernService.DeleteResearchQuestionAsync(id);
			return Ok("Xóa research question thành công.");
		}

		// ══════════════════════════ PICOC Elements ════════════════════════════

		/// <summary>
		/// Lấy PICOC element theo ID kèm child detail
		/// </summary>
		[HttpGet("picoc-elements/{id}")]
		public async Task<ActionResult<ApiResponse<PicocElementDto>>> GetPicocElementById(Guid id)
		{
			var result = await _coreGovernService.GetPicocElementByIdAsync(id);
			return Ok(result, "Lấy PICOC element thành công.");
		}

		/// <summary>
		/// Lấy toàn bộ PICOC elements của một research question
		/// </summary>
		[HttpGet("picoc-elements/research-question/{researchQuestionId}")]
		public async Task<ActionResult<ApiResponse<IEnumerable<PicocElementDto>>>> GetPicocElementsByResearchQuestion(
			Guid researchQuestionId)
		{
			var result = await _coreGovernService.GetPicocElementsByResearchQuestionIdAsync(researchQuestionId);
			return Ok(result, "Lấy danh sách PICOC elements thành công.");
		}

		/// <summary>
		/// Thêm PICOC element vào research question
		/// </summary>
		[HttpPost("picoc-elements")]
		public async Task<ActionResult<ApiResponse<PicocElementDto>>> AddPicocElement(
			[FromBody] AddPicocElementRequest request)
		{
			var result = await _coreGovernService.AddPicocElementAsync(request);
			return Created(result, "Thêm PICOC element thành công.");
		}

		/// <summary>
		/// Cập nhật PICOC element và child detail của nó
		/// </summary>
		[HttpPut("picoc-elements/{id}")]
		public async Task<ActionResult<ApiResponse<PicocElementDto>>> UpdatePicocElement(
			Guid id, [FromBody] UpdatePicocElementRequest request)
		{
			if (id != request.Id)
				throw new ArgumentException("Route ID không khớp với body ID.");

			var result = await _coreGovernService.UpdatePicocElementAsync(request);
			return Ok(result, "Cập nhật PICOC element thành công.");
		}

		/// <summary>
		/// Xóa PICOC element và child detail của nó
		/// </summary>
		[HttpDelete("picoc-elements/{id}")]
		public async Task<ActionResult<ApiResponse>> DeletePicocElement(Guid id)
		{
			await _coreGovernService.DeletePicocElementAsync(id);
			return Ok("Xóa PICOC element thành công.");
		}
	}
}
