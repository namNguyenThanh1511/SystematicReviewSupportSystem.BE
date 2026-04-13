using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.ExclusionReasonLibrary;
using SRSS.IAM.Services.ExclusionReasonLibraryService;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/exclusion-reason-libraries")]
    public class ExclusionReasonLibraryController : BaseController
    {
        private readonly IExclusionReasonLibraryService _service;

        public ExclusionReasonLibraryController(IExclusionReasonLibraryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all exclusion reasons from the global library with pagination and search.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ExclusionReasonLibraryDto>>>> GetAll([FromQuery] ExclusionReasonLibraryFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result, "Get list of exclusion reasons from library successfully.");
        }

        /// <summary>
        /// Create a new exclusion reason in the global library.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ExclusionReasonLibraryDto>>> Create([FromBody] CreateExclusionReasonRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Created(result, "Create new exclusion reason in library successfully.");
        }

        /// <summary>
        /// Create multiple exclusion reasons in the global library.
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ExclusionReasonLibraryDto>>>> BulkCreate([FromBody] List<CreateExclusionReasonRequest> requests)
        {
            var result = await _service.BulkCreateAsync(requests);
            return Created(result, "Bulk create exclusion reasons in library successfully.");
        }

        /// <summary>
        /// Toggle the active status of an exclusion reason in the global library.
        /// </summary>
        [HttpPatch("{id}/toggle-active")]
        public async Task<ActionResult<ApiResponse<ExclusionReasonLibraryDto>>> ToggleActive(Guid id)
        {
            var result = await _service.ToggleActiveAsync(id);
            return Ok(result, "Toggle exclusion reason status successfully.");
        }

        /// <summary>
        /// Hard delete an exclusion reason from the global library.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> HardDelete(Guid id)
        {
            await _service.HardDeleteAsync(id);
            return Ok("Delete exclusion reason from library successfully.");
        }
    }
}
