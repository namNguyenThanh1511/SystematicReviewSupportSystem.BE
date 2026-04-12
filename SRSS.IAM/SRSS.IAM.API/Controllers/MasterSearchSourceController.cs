using Microsoft.AspNetCore.Mvc;
using SRSS.IAM.Services.AdminMasterSourceService;
using SRSS.IAM.Services.DTOs.MasterSource;
using Shared.Models;
using Shared.Builder;

namespace SRSS.IAM.API.Controllers
{
    [ApiController]
    [Route("api/master-sources")]
    public class MasterSearchSourceController : BaseController
    {
        private readonly IMasterSearchSourceService _masterSourceService;

        public MasterSearchSourceController(IMasterSearchSourceService masterSourceService)
        {
            _masterSourceService = masterSourceService;
        }

        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AvailableMasterSearchSourceResponse>>>> GetAvailable()
        {
            var result = await _masterSourceService.GetAvailableAsync();
            return Ok(result, "Available master sources retrieved successfully.");
        }
    }
}
