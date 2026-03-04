using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Protocol;
using SRSS.IAM.Services.ProtocolService;

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
			await _protocolService.ApproveProtocolAsync(protocolId, request.ReviewerId);
			return Ok("Phê duyệt protocol thành công");
		}

		/// <summary>
		/// Xóa protocol và tất cả dữ liệu liên quan (cascade delete)
		/// </summary>
		[HttpDelete("{protocolId}")]
		public async Task<ActionResult<ApiResponse>> DeleteProtocol(Guid protocolId)
		{
			await _protocolService.DeleteProtocolAsync(protocolId);
			return Ok("Xóa protocol thành công");
		}
	}

	public class ApproveProtocolRequest
	{
		public Guid ReviewerId { get; set; }
	}
}