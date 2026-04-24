using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.DataExtraction;
using SRSS.IAM.Services.DataExtractionService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/data-extraction")]
    public class DataExtractionController : BaseController
    {
        private readonly IDataExtractionService _service;

        public DataExtractionController(IDataExtractionService service)
        {
            _service = service;
        }

        // ==================== Extraction Templates ====================

        /// <summary>
        /// Lấy tất cả Templates theo Data Extraction Process ID (với cấu trúc cây)
        /// </summary>
        [HttpGet("process/{processId}/templates")]
        public async Task<ActionResult<ApiResponse<List<ExtractionTemplateDto>>>> GetTemplatesByProcessId(
            Guid processId)
        {
            var result = await _service.GetTemplatesByProcessIdAsync(processId);
            return Ok(result, "Lấy danh sách templates thành công");
        }

        /// <summary>
        /// Lấy chi tiết 1 Template theo ID (với cấu trúc cây đầy đủ)
        /// </summary>
        [HttpGet("templates/{templateId}")]
        public async Task<ActionResult<ApiResponse<ExtractionTemplateDto>>> GetTemplateById(
            Guid templateId)
        {
            var result = await _service.GetTemplateByIdAsync(templateId);
            return Ok(result, "Lấy template thành công");
        }

        /// <summary>
        /// Tạo hoặc cập nhật Template (Upsert)
        /// </summary>
        [HttpPost("templates/upsert")]
        public async Task<ActionResult<ApiResponse<ExtractionTemplateDto>>> UpsertTemplate(
            [FromBody] ExtractionTemplateDto dto)
        {
            var result = await _service.UpsertTemplateAsync(dto);
            return Ok(result, "Lưu extraction template thành công");
        }

        /// <summary>
        /// Validate Template trước khi save (không lưu vào DB)
        /// </summary>
        [HttpPost("templates/validate")]
        public async Task<ActionResult<ApiResponse<TemplateValidationResultDto>>> ValidateTemplate(
            [FromBody] ExtractionTemplateDto dto)
        {
            var result = await _service.ValidateTemplateAsync(dto);
            return Ok(result, "Kiểm tra template hoàn tất");
        }

        /// <summary>
        /// Xóa một Extraction Template
        /// </summary>
        [HttpDelete("templates/{templateId}")]
        public async Task<ActionResult<ApiResponse>> DeleteTemplate(Guid templateId)
        {
            await _service.DeleteTemplateAsync(templateId);
            return Ok("Xóa template thành công");
        }

        /// <summary>
        /// AI gợi ý các trường extraction dựa trên tên section (RQ)
        /// </summary>
        [HttpPost("suggest-fields")]
        public async Task<ActionResult<ApiResponse<List<ExtractionFieldDto>>>> SuggestFields(
            [FromBody] SuggestFieldsRequestDto request)
        {
            var result = await _service.SuggestFieldsForSectionAsync(request.SectionName, request.ProjectContext ?? "");
            return Ok(result, "AI đã gợi ý các trường thành công");
        }
    }
}