using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Protocol;
using SRSS.IAM.Services.ProtocolService;
using System.Security.Claims;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/protocols")]
	//[Authorize]
	public class ProtocolController : BaseController
	{
		private readonly IProtocolService _protocolService;

		public ProtocolController(IProtocolService protocolService)
		{
			_protocolService = protocolService;
		}

		/// <summary>
		/// Tạo protocol mới cho một dự án systematic review
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<ApiResponse<ProtocolDetailResponse>>> CreateProtocol(
			[FromBody] CreateProtocolRequest request)
		{
			var result = await _protocolService.CreateProtocolAsync(request);
			return Created(result, "Tạo protocol thành công");
		}

		/// <summary>
		/// Cập nhật protocol (tự động tạo version nếu protocol đã được approved)
		/// </summary>
		[HttpPut]
		public async Task<ActionResult<ApiResponse<ProtocolDetailResponse>>> UpdateProtocol(
			[FromBody] UpdateProtocolRequest request)
		{
			var result = await _protocolService.UpdateProtocolAsync(request);
			return Ok(result, "Cập nhật protocol thành công");
		}

		/// <summary>
		/// Lấy chi tiết protocol bao gồm lịch sử version
		/// </summary>
		[HttpGet("{protocolId}")]
		public async Task<ActionResult<ApiResponse<ProtocolDetailResponse>>> GetProtocolById(Guid protocolId)
		{
			var result = await _protocolService.GetProtocolByIdAsync(protocolId);
			return Ok(result, "Lấy thông tin protocol thành công");
		}

		/// <summary>
		/// Lấy chi tiết đầy đủ protocol (bao gồm Study Characteristics, Search Sources, Study Selection, Quality Assessment, Data Extraction, Synthesis &amp; Dissemination)
		/// </summary>
		[HttpGet("{protocolId}/detail")]
		public async Task<ActionResult<ApiResponse<ProtocolDetailResponse>>> GetProtocolDetailById(Guid protocolId)
		{
			var result = await _protocolService.GetProtocolDetailByIdAsync(protocolId);
			return Ok(result, "Lấy chi tiết đầy đủ protocol thành công");
		}

		/// <summary>
		/// Lấy tất cả protocols của một project
		/// </summary>
		[HttpGet("project/{projectId}")]
		public async Task<ActionResult<ApiResponse<List<ProtocolDetailResponse>>>> GetProtocolsByProjectId(Guid projectId)
		{
			var result = await _protocolService.GetProtocolsByProjectIdAsync(projectId);
			return Ok(result, "Lấy danh sách protocols thành công");
		}

		/// <summary>
		/// Phê duyệt protocol
		/// </summary>
		[HttpPost("{protocolId}/approve")]
		public async Task<ActionResult<ApiResponse>> ApproveProtocol(Guid protocolId, [FromBody] ApproveProtocolRequest request)
		{
			// Get current user ID from JWT claims or use the provided one
			var currentUserId = GetCurrentUserId() ?? request.UserId;

			await _protocolService.ApproveProtocolAsync(protocolId, currentUserId);
			return Ok("Phê duyệt protocol thành công");
		}

		/// <summary>
		/// Xóa protocol mềm
		/// </summary>
		[HttpDelete("{protocolId}")]
		public async Task<ActionResult<ApiResponse>> DeleteProtocol(Guid protocolId)
		{
			await _protocolService.DeleteProtocolAsync(protocolId);
			return Ok("Xóa protocol thành công");
		}

		[HttpPost("{protocolId}/restore")]
		public async Task<ActionResult<ApiResponse>> RestoreProtocol(Guid protocolId)
		{
			await _protocolService.RestoreProtocolAsync(protocolId);
			return Ok("Khôi phục protocol thành công");
		}

		[HttpPost("{protocolId}/reject")]
		public async Task<ActionResult<ApiResponse>> RejectProtocol(Guid protocolId, [FromBody] RejectProtocolRequest request)
		{
			var currentUserId = GetCurrentUserId() ?? request.UserId;
			await _protocolService.RejectProtocolAsync(protocolId, currentUserId, request.Reason);
			return Ok("Từ chối protocol thành công");
		}

		[HttpPost("{protocolId}/submit")]
		public async Task<ActionResult<ApiResponse>> SubmitForReview(Guid protocolId)
		{
			await _protocolService.SubmitForReviewAsync(protocolId);
			return Ok("Submit protocol để review thành công");
		}

		/// <summary>
		/// Helper method để lấy user ID từ JWT token
		/// </summary>
		private Guid? GetCurrentUserId()
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
							  ?? User.FindFirst("sub")?.Value
							  ?? User.FindFirst("user_id")?.Value;

			if (Guid.TryParse(userIdClaim, out var userId))
			{
				return userId;
			}

			return null;
		}
	}

	public class ApproveProtocolRequest
	{
		public Guid UserId { get; set; }
	}
	public class RejectProtocolRequest
	{
		/// <summary>
		/// ID của user từ chối (optional nếu lấy từ JWT)
		/// </summary>
		public Guid UserId { get; set; }

		/// <summary>
		/// Lý do từ chối
		/// </summary>
		public string? Reason { get; set; }
	}
}