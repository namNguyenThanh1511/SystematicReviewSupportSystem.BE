using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.SelectionCriteria;
using SRSS.IAM.Services.StudyCharacteristicsService;

namespace SRSS.IAM.API.Controllers
{
	[ApiController]
	[Route("api/study-characteristics")]
	public class StudyCharacteristicsController : BaseController
	{
		private readonly IStudyCharacteristicsService _service;

		public StudyCharacteristicsController(IStudyCharacteristicsService service)
		{
			_service = service;
		}

		/// <summary>
		/// Upsert Study Characteristics for a Protocol
		/// </summary>
		[HttpPost("protocol/{protocolId}")]
		public async Task<ActionResult<ApiResponse<StudyCharacteristicsDto>>> UpsertCharacteristics(
			Guid protocolId, [FromBody] StudyCharacteristicsDto dto)
		{
			var result = await _service.UpsertCharacteristicsAsync(protocolId, dto);
			return Ok(result, "Study characteristics saved successfully");
		}

		/// <summary>
		/// Get Study Characteristics by Protocol ID
		/// </summary>
		[HttpGet("protocol/{protocolId}")]
		public async Task<ActionResult<ApiResponse<StudyCharacteristicsDto>>> GetByProtocolId(Guid protocolId)
		{
			var result = await _service.GetByProtocolIdAsync(protocolId);
			return Ok(result, "Study characteristics retrieved successfully");
		}

		/// <summary>
		/// Delete Study Characteristics for a Protocol
		/// </summary>
		[HttpDelete("protocol/{protocolId}")]
		public async Task<ActionResult<ApiResponse>> DeleteCharacteristics(Guid protocolId)
		{
			await _service.DeleteCharacteristicsAsync(protocolId);
			return Ok("Study characteristics deleted successfully");
		}
	}
}
