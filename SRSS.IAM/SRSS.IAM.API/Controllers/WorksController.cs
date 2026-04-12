using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.Crossref;
using SRSS.IAM.Services.DTOs.Crossref;

namespace SRSS.IAM.API.Controllers;

[ApiController]
[Route("api/works")]
public class WorksController : BaseController
{
    private readonly ICrossrefService _crossrefService;

    public WorksController(ICrossrefService crossrefService)
    {
        _crossrefService = crossrefService;
    }

    /// <summary>
    /// Search works on Crossref
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CrossrefMessageList<CrossrefWorkDto>>>> QueryWorks(
        [FromQuery] CrossrefQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await _crossrefService.GetWorksAsync(parameters, ct);
        return Ok(result, "Works retrieved successfully.");
    }

    /// <summary>
    /// Get work detail by DOI
    /// </summary>
    /// <param name="doi">The DOI of the work</param>
    [HttpGet("{**doi}")]
    public async Task<ActionResult<ApiResponse<CrossrefWorkDto>>> GetWorkDetail(string doi, CancellationToken ct)
    {
        doi = doi.Replace("%2F", "/");
        var result = await _crossrefService.GetWorkByDoiAsync(doi, ct);
        return Ok(result, "Work detail retrieved successfully.");
    }

    /// <summary>
    /// Get agency info by DOI
    /// </summary>
    [HttpGet("agency/{**doi}")]
    public async Task<ActionResult<ApiResponse<CrossrefAgencyDto>>> GetAgency(string doi, CancellationToken ct)
    {
        var result = await _crossrefService.GetAgencyByDoiAsync(doi, ct);
        return Ok(result, "Agency information retrieved successfully.");
    }
}
