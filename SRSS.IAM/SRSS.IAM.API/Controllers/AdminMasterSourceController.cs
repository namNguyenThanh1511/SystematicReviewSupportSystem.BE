using Microsoft.AspNetCore.Mvc;
using SRSS.IAM.Services.AdminMasterSourceService;
using SRSS.IAM.Services.DTOs.MasterSource;
using Shared.Models;
using Shared.Builder;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/admin/master-sources")]
    public class AdminMasterSourceController : BaseController
    {
        private readonly IMasterSearchSourceService _masterSourceService;

        public AdminMasterSourceController(IMasterSearchSourceService masterSourceService)
        {
            _masterSourceService = masterSourceService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MasterSearchSourceResponse>>>> GetAll([FromQuery] bool? isActive, [FromQuery] string? sourceName)
        {
            var result = await _masterSourceService.GetAllAsync(isActive, sourceName);
            return Ok(result, "Master sources retrieved successfully.");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MasterSearchSourceResponse>>> GetById(Guid id)
        {
            var result = await _masterSourceService.GetByIdAsync(id);
            return Ok(result, "Master source retrieved successfully.");
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MasterSearchSourceResponse>>> Create([FromBody] CreateMasterSearchSourceRequest request)
        {
            var result = await _masterSourceService.CreateAsync(request);
            return Ok(result, "Master source created successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<MasterSearchSourceResponse>>> Update(Guid id, [FromBody] UpdateMasterSearchSourceRequest request)
        {
            var result = await _masterSourceService.UpdateAsync(id, request);
            return Ok(result, "Master source updated successfully.");
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<MasterSearchSourceResponse>>> ToggleStatus(Guid id)
        {
            var result = await _masterSourceService.ToggleStatusAsync(id);
            return Ok(result, "Master source status toggled successfully.");
        }

        [HttpGet("{id}/usage")]
        public async Task<ActionResult<ApiResponse<int>>> GetUsageCount(Guid id)
        {
            var result = await _masterSourceService.GetUsageCountAsync(id);
            return Ok(result, "Usage count retrieved successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(Guid id)
        {
            await _masterSourceService.DeleteAsync(id);
            return Ok("Master source deleted successfully.");
        }
    }
}
